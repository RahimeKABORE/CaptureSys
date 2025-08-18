# CaptureSys v1.0.0

🚀 **Plateforme complète de capture documentaire avec architecture microservices**

## 🏗️ Architecture

### Services Core (5)
| Service | Port | Description | Status |
|---------|------|-------------|---------|
| **IngestionService** | 5001 | Ingestion de documents | ✅ |
| **OcrService** | 5002 | Reconnaissance optique | ✅ |
| **ExtractionService** | 5003 | Extraction de données | ✅ |
| **ClassificationService** | 5004 | Classification automatique | ✅ |
| **ExportService** | 5005 | Export multi-format | ✅ |

### Infrastructure (3)
| Service | Port | Description | Status |
|---------|------|-------------|---------|
| **ApiGateway** | 5000 | Passerelle principale | ✅ |
| **Gateway.Auth** | 5006 | Authentification JWT | ✅ |
| **Worker.Orchestrator** | 5007 | Orchestrateur de workflow | ✅ |

### Services Spécialisés (4)
| Service | Port | Description | Status |
|---------|------|-------------|---------|
| **ImageProcessorService** | 5008 | Préprocessing d'images | ✅ |
| **AutoLearningService** | 5009 | Apprentissage automatique | ✅ |
| **ScriptExecutionService** | 5010 | Exécution de scripts | ✅ |
| **TimerService** | 5011 | Tâches planifiées CRON | ✅ |

## 🚀 Démarrage rapide

```bash
# Cloner le repository
git clone https://github.com/VotreUsername/CaptureSys.git
cd CaptureSys

# Compiler tous les services
dotnet build

# Démarrer un service spécifique
dotnet run --project src/ApiGateway
dotnet run --project src/IngestionService/IngestionService.Api
# ... etc pour les autres services
```

## 🧪 Tests

```bash
# Tester les services (exemples)
curl http://localhost:5000/api/gateway          # ApiGateway
curl http://localhost:5001/api/ingestion        # IngestionService
curl http://localhost:5002/api/ocr              # OcrService
curl http://localhost:5008/api/ImageProcessor   # ImageProcessorService
curl http://localhost:5011/api/Timer            # TimerService
```

## 🏛️ Principes architecturaux

- ✅ **Clean Architecture** sur tous les services
- ✅ **Pattern Result** pour la gestion d'erreurs
- ✅ **Logging** intégré avec ILogger
- ✅ **APIs REST** complètes et documentées
- ✅ **Jobs asynchrones** pour les traitements longs
- ✅ **Microservices indépendants** et découplés

## 📁 Structure du projet

```
CaptureSys/
├── shared/                    # Composants partagés
├── src/
│   ├── ApiGateway/           # Passerelle principale
│   ├── IngestionService/     # Service d'ingestion
│   ├── OcrService/           # Service OCR
│   ├── ExtractionService/    # Service d'extraction
│   ├── ClassificationService/ # Service de classification
│   ├── ExportService/        # Service d'export
│   ├── Gateway.Auth/         # Service d'authentification
│   ├── Worker.Orchestrator/  # Orchestrateur
│   ├── ImageProcessorService/ # Traitement d'images
│   ├── AutoLearningService/  # Apprentissage automatique
│   ├── ScriptExecutionService/ # Exécution de scripts
│   └── TimerService/         # Planificateur CRON
└── README.md
```

## 🔧 Technologies

- **.NET 9.0** - Framework principal
- **ASP.NET Core** - APIs REST
- **Quartz.NET** - Planification de tâches
- **Clean Architecture** - Pattern architectural
- **Result Pattern** - Gestion d'erreurs

## 📈 Prochaines étapes

- [ ] Interfaces utilisateur (CompletionUI, AdministrationUI)
- [ ] Intégration des services via l'orchestrateur
- [ ] Tests de bout en bout
- [ ] Documentation technique détaillée
- [ ] CI/CD Pipeline

---

**CaptureSys v1.0.0** - Architecture microservices complète et fonctionnelle ! 🎯
