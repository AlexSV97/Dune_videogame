"""
Dune-Arrakis-Dominion - CrewAI Monthly Decision Agent
====================================================

Este script implementa agentes CrewAI que analizan el estado del juego
y proporcionan recomendaciones estratégicas para los jugadores.

Uso:
    python crewai_monthly_decision.py
    
El servidor FastAPI estará disponible en http://localhost:5000
"""

import os
import sys
import json
import logging
from typing import Dict, List, Optional, Any
from datetime import datetime
from pathlib import Path

from crewai import Agent, Task, Crew, Process
from crewai.tools import BaseTool
from pydantic import BaseModel, Field

import uvicorn
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from fastapi import Request

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="Dune Arrakis Dominion AI API",
    description="API para recomendaciones estratégicas del juego Dune",
    version="1.0.0"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

GAME_STATE_FILE = Path("game_state.json")
MEMORY_DIR = Path("memory")
MEMORY_DIR.mkdir(exist_ok=True)


class GameStateAnalyzerTool(BaseTool):
    name: str = "game_state_analyzer"
    description: str = "Analiza el estado actual del juego y extrae información clave sobre recursos, instalaciones y criaturas."

    def _run(self, game_state: Dict[str, Any]) -> Dict[str, Any]:
        analysis = {
            "total_resources": {},
            "resource_ratios": {},
            "facilities_summary": {},
            "creatures_summary": {},
            "enclaves_summary": {},
            "warnings": [],
            "opportunities": []
        }
        
        resources = game_state.get("resources", {})
        
        analysis["total_resources"] = {
            "spice": resources.get("spice", 0),
            "water": resources.get("water", 0),
            "credits": resources.get("credits", 0),
            "knowledge": resources.get("knowledge", 0),
            "population": resources.get("population", 0)
        }
        
        spice = resources.get("spice", 0)
        water = resources.get("water", 0)
        credits = resources.get("credits", 0)
        
        if water > 0:
            analysis["resource_ratios"]["spice_to_water"] = spice / water
        
        if credits > 0:
            analysis["resource_ratios"]["spice_to_credits"] = spice / credits
        
        if resources.get("water", 0) < resources.get("spice", 0) * 0.3:
            analysis["warnings"].append(
                "Reservas de agua bajas en relación a la especia. "
                "Considera construir WindTraps o WaterExtractors."
            )
        
        facilities = game_state.get("facilities", [])
        operational = sum(1 for f in facilities if f.get("operational", True))
        
        analysis["facilities_summary"] = {
            "total": len(facilities),
            "operational": operational,
            "by_type": self._count_by_type(facilities)
        }
        
        if operational == 0:
            analysis["warnings"].append(
                "No hay instalaciones operativas. Prioriza la construcción."
            )
        
        if len(facilities) > 0 and operational / len(facilities) < 0.5:
            analysis["warnings"].append(
                "Más del 50% de las instalaciones no están operativas."
            )
        
        creatures = game_state.get("activeCreatures", 0)
        if creatures > 5:
            analysis["warnings"].append(
                "Muchas criaturas sin domar. Considera domarlas o usar sus servicios."
            )
        
        analysis["opportunities"] = self._identify_opportunities(game_state)
        
        return analysis
    
    def _count_by_type(self, facilities: List[Dict]) -> Dict[str, int]:
        counts = {}
        for f in facilities:
            f_type = f.get("type", "Unknown")
            counts[f_type] = counts.get(f_type, 0) + 1
        return counts
    
    def _identify_opportunities(self, game_state: Dict) -> List[str]:
        opportunities = []
        
        resources = game_state.get("resources", {})
        facilities = game_state.get("facilities", [])
        
        if resources.get("spice", 0) > 5000 and not any(
            f.get("type") == "SpiceRefinery" for f in facilities
        ):
            opportunities.append(
                "Alta producción de especia disponible. "
                "Un SpiceRefinery maximizaría las ganancias."
            )
        
        if resources.get("water", 0) < 1000 and not any(
            f.get("type") in ["WindTrap", "WaterExtractor"] for f in facilities
        ):
            opportunities.append(
                "Escasez de agua detectada. "
                "WindTrap o WaterExtractor son prioritarios."
            )
        
        return opportunities


class StrategicRecommendationTool(BaseTool):
    name: str = "strategic_recommender"
    description: str = "Genera recomendaciones estratégicas basadas en el análisis del juego."

    def _run(self, analysis: Dict[str, Any], difficulty: str) -> Dict[str, Any]:
        recommendations = {
            "priority_actions": [],
            "long_term_strategy": {},
            "risk_assessment": {}
        }
        
        warnings = analysis.get("warnings", [])
        opportunities = analysis.get("opportunities", [])
        resources = analysis.get("total_resources", {})
        
        if resources.get("credits", 0) < 1000:
            recommendations["priority_actions"].append({
                "action": "conserve_credits",
                "title": "Conservar Créditos",
                "description": "Los créditos están bajos. Evita construcciones costosas.",
                "urgency": "high",
                "confidence": 0.9
            })
        
        if resources.get("spice", 0) > 3000:
            recommendations["priority_actions"].append({
                "action": "invest_spice",
                "title": "Invertir Especia",
                "description": "Stockpile de especia saludable. Considera intercambios.",
                "urgency": "medium",
                "confidence": 0.8
            })
        
        if any("water" in w.lower() for w in warnings):
            recommendations["priority_actions"].append({
                "action": "build_water_infrastructure",
                "title": "Infraestructura de Agua",
                "description": "Construir WindTrap o WaterExtractor es prioritario.",
                "urgency": "critical",
                "confidence": 0.95
            })
        
        for opp in opportunities:
            recommendations["priority_actions"].append({
                "action": "seize_opportunity",
                "title": "Oportunidad Detectada",
                "description": opp,
                "urgency": "medium",
                "confidence": 0.7
            })
        
        recommendations["long_term_strategy"] = {
            "difficulty": difficulty,
            "primary_goal": self._determine_primary_goal(difficulty, resources),
            "secondary_goals": self._determine_secondary_goals(resources)
        }
        
        recommendations["risk_assessment"] = {
            "resource_shortage_risk": self._calculate_risk("shortage", resources),
            "military_risk": self._calculate_risk("military", analysis),
            "economic_risk": self._calculate_risk("economic", resources)
        }
        
        return recommendations
    
    def _determine_primary_goal(self, difficulty: str, resources: Dict) -> str:
        if difficulty == "Messiah":
            return "Sobrevivir y construir influencia antes de ser eliminado."
        elif resources.get("credits", 0) < 5000:
            return "Establecer base económica sólida."
        else:
            return "Expandir dominio y设施."
    
    def _determine_secondary_goals(self, resources: Dict) -> List[str]:
        goals = []
        if resources.get("water", 0) < 2000:
            goals.append("Garantizar suministro de agua.")
        if resources.get("knowledge", 0) < 100:
            goals.append("Desarrollar investigación.")
        return goals
    
    def _calculate_risk(self, risk_type: str, data: Any) -> str:
        if risk_type == "shortage":
            resources = data
            if resources.get("water", 0) < 500 or resources.get("spice", 0) < 500:
                return "high"
            elif resources.get("water", 0) < 1000 or resources.get("spice", 0) < 1000:
                return "medium"
            return "low"
        elif risk_type == "military":
            analysis = data
            facilities = analysis.get("facilities_summary", {})
            if facilities.get("total", 0) < 2:
                return "high"
            return "medium"
        elif risk_type == "economic":
            resources = data
            if resources.get("credits", 0) < 1000:
                return "high"
            elif resources.get("credits", 0) < 5000:
                return "medium"
            return "low"
        return "unknown"


class MentatFinancieroAgent:
    def __init__(self):
        self.analyzer = GameStateAnalyzerTool()
        self.recommender = StrategicRecommendationTool()
    
    def analyze_and_recommend(self, game_state: Dict) -> Dict[str, Any]:
        logger.info(f"Mentat Financiero analizando estado del juego...")
        
        analysis = self.analyzer._run(game_state)
        
        difficulty = game_state.get("difficulty", "Standard")
        recommendations = self.recommender._run(analysis, difficulty)
        
        result = {
            "agent_name": "Mentat Financiero",
            "timestamp": datetime.now().isoformat(),
            "analysis": analysis,
            "recommendations": recommendations,
            "executive_summary": self._generate_summary(analysis, recommendations)
        }
        
        self._save_to_memory(result)
        
        return result
    
    def _generate_summary(self, analysis: Dict, recommendations: Dict) -> str:
        warnings = analysis.get("warnings", [])
        actions = recommendations.get("priority_actions", [])
        
        summary = "## Resumen Ejecutivo\n\n"
        
        if warnings:
            summary += "### Alertas:\n"
            for w in warnings[:3]:
                summary += f"- {w}\n"
            summary += "\n"
        
        if actions:
            summary += "### Acciones Prioritarias:\n"
            critical = [a for a in actions if a.get("urgency") == "critical"]
            high = [a for a in actions if a.get("urgency") == "high"]
            
            for a in critical[:2]:
                summary += f"- **[CRÍTICO]** {a['title']}: {a['description']}\n"
            for a in high[:2]:
                summary += f"- {a['title']}: {a['description']}\n"
        
        return summary
    
    def _save_to_memory(self, result: Dict):
        filename = MEMORY_DIR / f"mentat_financiero_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
        with open(filename, 'w', encoding='utf-8') as f:
            json.dump(result, f, indent=2, ensure_ascii=False)
        logger.info(f"Análisis guardado en {filename}")


class MaestroBestiasAgent:
    def __init__(self):
        self.analyzer = GameStateAnalyzerTool()
    
    def analyze_and_recommend(self, game_state: Dict) -> Dict[str, Any]:
        logger.info(f"Maestro de Bestias analizando criaturas...")
        
        creatures_count = game_state.get("activeCreatures", 0)
        resources = game_state.get("resources", {})
        facilities = game_state.get("facilities", [])
        
        recommendations = {
            "creature_management": [],
            "taming_priorities": [],
            "combat_readiness": []
        }
        
        if creatures_count == 0:
            recommendations["creature_management"].append({
                "action": "acquire_creatures",
                "title": "Adquirir Criaturas",
                "description": "Sin criaturas, se pierden beneficios defensivos y de transporte.",
                "urgency": "high"
            })
        
        worm_count = sum(1 for f in facilities if "worm" in str(f.get("type", "")).lower())
        if worm_count > 0:
            recommendations["creature_management"].append({
                "action": "worm_benefits",
                "title": "Gusanos de Arena",
                "description": "Los Gusanos proporcionan protección excepcional y control del territorio.",
                "urgency": "info"
            })
        
        if resources.get("water", 0) > 500 and resources.get("spice", 0) > 200:
            recommendations["taming_priorities"].append({
                "action": "start_taming",
                "title": "Iniciar Domadura",
                "description": "Recursos suficientes para comenzar proceso de domadura.",
                "creature_types": ["Dewback", "FremenRider"],
                "urgency": "medium"
            })
        
        recommendations["combat_readiness"].append({
            "action": "evaluate_forces",
            "title": "Evaluación de Fuerzas",
            "description": f"Actualmente hay {creatures_count} criaturas disponibles.",
            "recommendation": "Prioriza Domadores Fremen para movilidad."
        })
        
        result = {
            "agent_name": "Maestro de Bestias",
            "timestamp": datetime.now().isoformat(),
            "creatures_count": creatures_count,
            "recommendations": recommendations,
            "executive_summary": self._generate_summary(recommendations)
        }
        
        self._save_to_memory(result)
        
        return result
    
    def _generate_summary(self, recommendations: Dict) -> str:
        summary = "## Resumen - Maestro de Bestias\n\n"
        
        mgmt = recommendations.get("creature_management", [])
        if mgmt:
            summary += "### Gestión de Criaturas:\n"
            for m in mgmt[:3]:
                summary += f"- **{m['title']}**: {m['description']}\n"
        
        taming = recommendations.get("taming_priorities", [])
        if taming:
            summary += "\n### Prioridades de Domadura:\n"
            for t in taming[:2]:
                summary += f"- **{t['title']}**: {t['description']}\n"
        
        return summary
    
    def _save_to_memory(self, result: Dict):
        filename = MEMORY_DIR / f"maestro_bestias_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
        with open(filename, 'w', encoding='utf-8') as f:
            json.dump(result, f, indent=2, ensure_ascii=False)
        logger.info(f"Análisis guardado en {filename}")


mentat_financiero = MentatFinancieroAgent()
maestro_bestias = MaestroBestiasAgent()


@app.get("/")
async def root():
    return {
        "service": "Dune Arrakis Dominion AI API",
        "version": "1.0.0",
        "status": "operational",
        "endpoints": {
            "/api/analyze": "POST - Análisis completo del juego",
            "/api/mentat/financial": "POST - Recomendaciones financieras",
            "/api/beastmaster/advice": "POST - Consejos sobre criaturas",
            "/api/health": "GET - Estado del servicio"
        }
    }


@app.get("/api/health")
async def health_check():
    return {
        "status": "healthy",
        "timestamp": datetime.now().isoformat(),
        "agents_available": ["Mentat Financiero", "Maestro de Bestias"]
    }


@app.post("/api/analyze")
async def analyze_game_state(request: Request):
    try:
        game_state = await request.json()
        
        logger.info(f"Recibido estado del juego: {game_state.get('playerName', 'Unknown')}")
        
        financial_analysis = mentat_financiero.analyze_and_recommend(game_state)
        beast_analysis = maestro_bestias.analyze_and_recommend(game_state)
        
        combined_result = {
            "timestamp": datetime.now().isoformat(),
            "month": game_state.get("currentMonth", 1),
            "year": game_state.get("currentYear", 10256),
            "financial_analysis": financial_analysis,
            "beast_analysis": beast_analysis,
            "unified_recommendations": _merge_recommendations(
                financial_analysis.get("recommendations", {}),
                beast_analysis.get("recommendations", {})
            )
        }
        
        return JSONResponse(content=combined_result)
    
    except Exception as e:
        logger.error(f"Error en análisis: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/mentat/financial")
async def mentat_financial_advice(request: Request):
    try:
        game_state = await request.json()
        
        logger.info(f"Mentat Financiero recibiendo solicitud...")
        
        result = mentat_financiero.analyze_and_recommend(game_state)
        
        recommendation = _create_recommendation(
            "Mentat Financiero",
            result.get("recommendations", {}).get("priority_actions", [])[:3],
            result.get("executive_summary", "")
        )
        
        return JSONResponse(content=recommendation)
    
    except Exception as e:
        logger.error(f"Error en Mentat Financiero: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/beastmaster/advice")
async def beastmaster_advice(request: Request):
    try:
        game_state = await request.json()
        
        logger.info(f"Maestro de Bestias recibiendo solicitud...")
        
        result = maestro_bestias.analyze_and_recommend(game_state)
        
        recommendation = _create_recommendation(
            "Maestro de Bestias",
            _flatten_beast_recommendations(result.get("recommendations", {})),
            result.get("executive_summary", "")
        )
        
        return JSONResponse(content=recommendation)
    
    except Exception as e:
        logger.error(f"Error en Maestro de Bestias: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))


def _create_recommendation(agent_name: str, actions: List[Dict], reasoning: str) -> Dict:
    priority = "Low"
    if any(a.get("urgency") == "critical" for a in actions):
        priority = "Critical"
    elif any(a.get("urgency") == "high" for a in actions):
        priority = "High"
    elif any(a.get("urgency") == "medium" for a in actions):
        priority = "Medium"
    
    primary_action = actions[0] if actions else {}
    
    return {
        "agentName": agent_name,
        "priority": priority,
        "actionType": primary_action.get("action", "unknown"),
        "title": primary_action.get("title", "Sin título"),
        "description": primary_action.get("description", ""),
        "reasoning": reasoning,
        "confidence": primary_action.get("confidence", 0.5),
        "allActions": actions
    }


def _flatten_beast_recommendations(beast_recs: Dict) -> List[Dict]:
    flattened = []
    for category, items in beast_recs.items():
        if isinstance(items, list):
            flattened.extend(items)
    return flattened[:3]


def _merge_recommendations(financial: Dict, beast: Dict) -> List[Dict]:
    merged = []
    
    for action in financial.get("priority_actions", []):
        action["source"] = "Mentat Financiero"
        merged.append(action)
    
    for category in ["creature_management", "taming_priorities", "combat_readiness"]:
        for action in beast.get(category, []):
            action["source"] = "Maestro de Bestias"
            merged.append(action)
    
    priority_order = {"critical": 0, "high": 1, "medium": 2, "low": 3, "info": 4}
    merged.sort(key=lambda x: priority_order.get(x.get("urgency", "low"), 5))
    
    return merged[:10]


def run_api():
    logger.info("Iniciando Dune Arrakis Dominion AI API...")
    logger.info("Endpoints disponibles:")
    logger.info("  - POST /api/analyze")
    logger.info("  - POST /api/mentat/financial")
    logger.info("  - POST /api/beastmaster/advice")
    
    uvicorn.run(app, host="0.0.0.0", port=5000, log_level="info")


if __name__ == "__main__":
    run_api()
