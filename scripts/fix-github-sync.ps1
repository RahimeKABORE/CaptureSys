#!/usr/bin/env pwsh

Write-Host "Resolution du conflit GitHub pour CaptureSys..." -ForegroundColor Green

# Se placer dans le repertoire racine du projet
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath
Set-Location $projectRoot

Write-Host "Repertoire de travail: $projectRoot" -ForegroundColor Yellow

# Revenir a l'URL HTTPS pour eviter les problemes SSH
Write-Host "Configuration de l'origine HTTPS..." -ForegroundColor Blue
git remote set-url origin https://github.com/RahimeKABORE/CaptureSys.git

# Recuperer les changements du depot distant
Write-Host "Recuperation des changements du depot distant..." -ForegroundColor Blue
git fetch origin

# Integrer les changements distants avec strategie de merge
Write-Host "Integration des changements..." -ForegroundColor Blue
git pull origin main --allow-unrelated-histories

if ($LASTEXITCODE -eq 0) {
    Write-Host "Integration reussie!" -ForegroundColor Green
} else {
    Write-Host "Conflit detecte. Resolution automatique..." -ForegroundColor Yellow
    # En cas de conflit, privilégier notre version
    git merge origin/main --allow-unrelated-histories --strategy-option=ours
}

# Pousser nos changements
Write-Host "Push de la branche main vers GitHub..." -ForegroundColor Blue
git push -u origin main

if ($LASTEXITCODE -eq 0) {
    Write-Host "Push reussi!" -ForegroundColor Green
} else {
    Write-Host "Tentative de push force (attention: efface l'historique distant)..." -ForegroundColor Yellow
    $response = Read-Host "Voulez-vous forcer le push? Cela va ecraser le depot GitHub (y/n)"
    if ($response -eq "y") {
        git push -u origin main --force
        Write-Host "Push force effectue!" -ForegroundColor Green
    } else {
        Write-Host "Push annule" -ForegroundColor Red
        exit 1
    }
}

# Pousser toutes les autres branches
Write-Host "Push des autres branches..." -ForegroundColor Blue
git push --all origin

# Afficher le statut final
Write-Host ""
Write-Host "Statut final:" -ForegroundColor Yellow
git remote -v
git branch -a
git log --oneline -3

Write-Host ""
Write-Host "Synchronisation GitHub terminee!" -ForegroundColor Green
Write-Host "Visitez: https://github.com/RahimeKABORE/CaptureSys" -ForegroundColor Cyan

Read-Host "Appuyez sur Entree pour fermer"
