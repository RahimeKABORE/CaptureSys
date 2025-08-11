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

# Verifier le statut actuel
Write-Host "Statut actuel des remotes:" -ForegroundColor Cyan
git remote -v

# Recuperer les changements du depot distant
Write-Host "Recuperation des changements du depot distant..." -ForegroundColor Blue
git fetch origin

# Verifier s'il y a des differences
$remoteMain = git rev-parse origin/main 2>$null
$localMain = git rev-parse main 2>$null

if ($remoteMain -and $localMain -and $remoteMain -ne $localMain) {
    Write-Host "Differences detectees entre local et distant" -ForegroundColor Yellow
    
    # Integrer les changements distants avec strategie de merge
    Write-Host "Integration des changements avec strategie ours..." -ForegroundColor Blue
    git pull origin main --allow-unrelated-histories --strategy-option=ours
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Echec du merge automatique. Tentative de merge manuel..." -ForegroundColor Yellow
        git merge origin/main --allow-unrelated-histories
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Conflit manuel detecte. Utilisation de notre version..." -ForegroundColor Yellow
            git reset --hard HEAD
        }
    }
} else {
    Write-Host "Pas de differences majeures detectees" -ForegroundColor Green
}

# Pousser nos changements
Write-Host "Push de la branche main vers GitHub..." -ForegroundColor Blue
git push -u origin main

if ($LASTEXITCODE -eq 0) {
    Write-Host "Push de main reussi!" -ForegroundColor Green
} else {
    Write-Host "Echec du push normal. Proposition de push force..." -ForegroundColor Yellow
    Write-Host "ATTENTION: Le push force va ecraser l'historique distant!" -ForegroundColor Red
    $response = Read-Host "Voulez-vous forcer le push? (y/n)"
    
    if ($response.ToLower() -eq "y") {
        git push -u origin main --force
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Push force effectue avec succes!" -ForegroundColor Green
        } else {
            Write-Host "Echec du push force. Verifiez vos permissions GitHub." -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "Push annule par l'utilisateur" -ForegroundColor Yellow
        exit 1
    }
}

# Pousser toutes les autres branches
Write-Host "Push des autres branches..." -ForegroundColor Blue
git push --all origin

# Afficher le statut final
Write-Host ""
Write-Host "=== STATUT FINAL ===" -ForegroundColor Yellow
Write-Host "Remotes:" -ForegroundColor Cyan
git remote -v
Write-Host ""
Write-Host "Branches:" -ForegroundColor Cyan
git branch -a
Write-Host ""
Write-Host "Derniers commits:" -ForegroundColor Cyan
git log --oneline -3

Write-Host ""
Write-Host "=== SYNCHRONISATION TERMINEE ===" -ForegroundColor Green
Write-Host "Depot GitHub: https://github.com/RahimeKABORE/CaptureSys" -ForegroundColor Cyan
Write-Host "Vous devriez maintenant voir tous vos fichiers sur GitHub!" -ForegroundColor Green

Read-Host "Appuyez sur Entree pour fermer"
