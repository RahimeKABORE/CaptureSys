#!/usr/bin/env pwsh

Write-Host "Initialisation du depot Git pour CaptureSys..." -ForegroundColor Green

# Verifier si Git est installe
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Host "ERREUR: Git n'est pas installe ou n'est pas dans le PATH" -ForegroundColor Red
    Write-Host "Telechargez Git depuis: https://git-scm.com/download/windows" -ForegroundColor Yellow
    Read-Host "Appuyez sur Entree pour quitter"
    exit 1
}

# Verifier la version de Git
$gitVersion = git --version
Write-Host "Git detecte: $gitVersion" -ForegroundColor Green

# Se placer dans le repertoire racine du projet
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath
Set-Location $projectRoot

Write-Host "Repertoire de travail: $projectRoot" -ForegroundColor Yellow

# Verifier que nous sommes bien dans le bon repertoire
if (-not (Test-Path "Directory.Build.props")) {
    Write-Host "ERREUR: Impossible de trouver Directory.Build.props dans ce repertoire" -ForegroundColor Red
    Write-Host "Assurez-vous d'etre dans le repertoire racine de CaptureSys" -ForegroundColor Yellow
    Read-Host "Appuyez sur Entree pour quitter"
    exit 1
}

# Initialiser le depot Git si ce n'est pas deja fait
if (-not (Test-Path ".git")) {
    Write-Host "Initialisation du depot Git..." -ForegroundColor Blue
    git init
    git config init.defaultBranch main
    git checkout -b main
    Write-Host "Depot Git initialise avec la branche 'main'" -ForegroundColor Green
} else {
    Write-Host "Depot Git deja initialise" -ForegroundColor Green
    $currentBranch = git branch --show-current
    Write-Host "Branche actuelle: $currentBranch" -ForegroundColor Cyan
}

# Ajouter tous les fichiers
Write-Host "Ajout des fichiers au staging..." -ForegroundColor Blue
git add .

# Verifier s'il y a deja des commits
$hasCommits = git log --oneline -1 2>$null
if ($null -eq $hasCommits) {
    Write-Host "Creation du commit initial..." -ForegroundColor Blue
    git commit -m "Initial commit - CaptureSys project structure"
} else {
    Write-Host "Commit des changements..." -ForegroundColor Blue
    git commit -m "Update project structure and configuration"
}

# Creer les branches de developpement
Write-Host "Creation des branches de developpement..." -ForegroundColor Blue

$branches = @("develop", "feature/api-gateway", "feature/auth-service", "feature/ocr-pipeline", "feature/ui-components", "release/v1.0.0")

foreach ($branch in $branches) {
    $existingBranch = git branch --list $branch
    if ($null -eq $existingBranch -or $existingBranch -eq "") {
        git checkout -b $branch
        Write-Host "  Branche '$branch' creee" -ForegroundColor Green
    } else {
        Write-Host "  Branche '$branch' existe deja" -ForegroundColor Yellow
    }
}

# Retourner sur main
git checkout main

# Afficher le statut
Write-Host ""
Write-Host "Statut du depot:" -ForegroundColor Yellow
git branch -a
git log --oneline -5

Write-Host ""
Write-Host "Depot Git initialise avec succes!" -ForegroundColor Green
Write-Host "Branches disponibles:" -ForegroundColor Yellow
Write-Host "  - main (branche principale)" -ForegroundColor Cyan
Write-Host "  - develop (developpement)" -ForegroundColor Cyan  
Write-Host "  - feature/* (fonctionnalites)" -ForegroundColor Cyan
Write-Host "  - release/* (versions)" -ForegroundColor Cyan

Write-Host ""
Write-Host "Prochaines etapes:" -ForegroundColor Yellow
Write-Host "  1. git remote add origin <url-du-repo>" -ForegroundColor White
Write-Host "  2. git push -u origin main" -ForegroundColor White
Write-Host "  3. git push --all origin" -ForegroundColor White

Write-Host ""
Write-Host "Script termine avec succes!" -ForegroundColor Green
Read-Host "Appuyez sur Entree pour fermer"
