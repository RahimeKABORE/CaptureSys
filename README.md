# CaptureSys - Intelligent Document Capture System

> Système moderne de capture intelligente de documents pour remplacer Captiva, basé sur .NET 9 et Clean Architecture.

## 🚀 Démarrage rapide

### 1. Initialisation Git

```bash
# Windows (PowerShell)
.\scripts\init-git.ps1

# Linux/Mac
chmod +x scripts/init-git.sh
./scripts/init-git.sh
```

### 2. Configuration du dépôt distant

```bash
git remote add origin https://github.com/votre-org/capturesys.git
git push -u origin main
git push --all origin
```

### 3. Lancement avec Docker

```bash
cd docker
docker-compose up -d
```

## 🏗️ Architecture

### Microservices
- **ApiGateway** - Point d'entrée REST principal (port 8080)
- **Gateway.Auth** - Service d'authentification JWT (port 8081)
- **CompletionUI** - Interface Blazor de validation OCR (port 8082)  
- **AdministrationUI** - Interface d'administration (port 8083)
- **IngestionService** - Ingestion de documents
- **OcrService** - Extraction OCR avec Tesseract
- **ClassificationService** - Classification de documents ML.NET
- **ExtractionService** - Extraction de champs par zones
- **ExportService** - Export vers PostgreSQL/CSV/S3
- **Worker.Orchestrator** - Orchestrateur de traitement

### Infrastructure
- **PostgreSQL** - Base de données principale
- **RabbitMQ** - Message broker pour communication async
- **Redis** - Cache distribué

## 🌿 Workflow Git

```
main ← develop ← feature/xxx
    ← release/vX.X.X
```

### Branches
- `main` - Production stable
- `develop` - Intégration continue  
- `feature/*` - Développement de fonctionnalités
- `release/*` - Préparation des versions
- `hotfix/*` - Corrections urgentes

## 📦 Structure du projet

```
CaptureSys/
├── src/                    # Microservices
├── shared/                 # Code partagé
├── recognition-projects/   # Projets OCR configurables
├── tests/                  # Tests d'intégration
├── docker/                 # Configuration Docker
├── scripts/                # Scripts utilitaires
└── docs/                   # Documentation
```

## 🧪 Tests

```bash
# Tests unitaires
dotnet test

# Tests d'intégration
dotnet test tests/Integration/

# Tests de charge
dotnet test tests/LoadTests/
```

## 📖 Documentation

- [Architecture](docs/architecture.md)
- [API Reference](docs/api/)
- [Configuration OCR](docs/ocr-setup.md)
- [Déploiement](docs/deployment.md)
