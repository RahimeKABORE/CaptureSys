#!/bin/bash

echo "🚀 Initialisation du dépôt Git pour CaptureSys..."

# Vérifier si Git est installé
if ! command -v git &> /dev/null; then
    echo "❌ Git n'est pas installé"
    exit 1
fi

# Se placer dans le répertoire racine du projet
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
cd "$PROJECT_ROOT"

echo "📁 Répertoire de travail: $PROJECT_ROOT"

# Initialiser le dépôt Git si ce n'est pas déjà fait
if [ ! -d ".git" ]; then
    echo "🔧 Initialisation du dépôt Git..."
    git init
    
    # Configurer la branche principale comme 'main'
    git config init.defaultBranch main
    git checkout -b main 2>/dev/null || git branch -M main
else
    echo "✅ Dépôt Git déjà initialisé"
fi

# Ajouter tous les fichiers
echo "📦 Ajout des fichiers au staging..."
git add .

# Premier commit
echo "💾 Création du commit initial..."
git commit -m "🎉 Initial commit - CaptureSys project structure

- Solution .NET 9 avec Clean Architecture
- 14 microservices configurés
- Docker Compose avec PostgreSQL, RabbitMQ, Redis  
- Structure recognition-projects pour projets OCR
- Tests unitaires et d'intégration
- Configuration CI/CD GitHub Actions"

# Créer les branches de développement
echo "🌿 Création des branches de développement..."

branches=(
    "develop"
    "feature/api-gateway" 
    "feature/auth-service"
    "feature/ocr-pipeline"
    "feature/ui-components"
    "release/v1.0.0"
)

for branch in "${branches[@]}"; do
    git checkout -b "$branch"
    echo "  ✅ Branche '$branch' créée"
done

# Retourner sur main
git checkout main

# Afficher le statut
echo ""
echo "📊 Statut du dépôt:"
git branch -a
git log --oneline -5

echo ""
echo "🎯 Dépôt Git initialisé avec succès!"
echo "Branches disponibles:"
echo "  - main (branche principale)"
echo "  - develop (développement)"  
echo "  - feature/* (fonctionnalités)"
echo "  - release/* (versions)"

echo ""
echo "💡 Prochaines étapes:"
echo "  1. git remote add origin <url-du-repo>"
echo "  2. git push -u origin main"
echo "  3. git push --all origin"
