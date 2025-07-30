#!/usr/bin/env pwsh

Write-Host "Push du depot CaptureSys vers GitHub..." -ForegroundColor Green

# Se placer dans le repertoire racine du projet
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath
Set-Location $projectRoot

Write-Host "Repertoire de travail: $projectRoot" -ForegroundColor Yellow

# Verifier que le depot Git existe
if (-not (Test-Path ".git")) {
    Write-Host "ERREUR: Aucun depot Git trouve. Executez d'abord init-git.ps1" -ForegroundColor Red
    Read-Host "Appuyez sur Entree pour quitter"
    exit 1
}

# Verifier la branche actuelle
$currentBranch = git branch --show-current
Write-Host "Branche actuelle: $currentBranch" -ForegroundColor Cyan

# Ajouter l'origine GitHub si elle n'existe pas
$remoteOrigin = git remote get-url origin 2>$null
if ($null -eq $remoteOrigin) {
    Write-Host "Ajout de l'origine GitHub..." -ForegroundColor Blue
    git remote add origin https://github.com/RahimeKABORE/CaptureSys.git
    Write-Host "Origine GitHub ajoutee" -ForegroundColor Green
} else {
    Write-Host "Origine GitHub deja configuree: $remoteOrigin" -ForegroundColor Yellow
}

# Pousser la branche main
Write-Host "Push de la branche main vers GitHub..." -ForegroundColor Blue
git push -u origin main

if ($LASTEXITCODE -eq 0) {
    Write-Host "Branche main poussee avec succes!" -ForegroundColor Green
} else {
    Write-Host "ERREUR lors du push de main" -ForegroundColor Red
    Write-Host "Verifiez vos credentials GitHub" -ForegroundColor Yellow
    Read-Host "Appuyez sur Entree pour continuer quand meme"
}

# Pousser toutes les autres branches
Write-Host "Push de toutes les branches vers GitHub..." -ForegroundColor Blue
git push --all origin

if ($LASTEXITCODE -eq 0) {
    Write-Host "Toutes les branches poussees avec succes!" -ForegroundColor Green
} else {
    Write-Host "ERREUR lors du push de certaines branches" -ForegroundColor Yellow
}

# Afficher le statut final
Write-Host ""
Write-Host "Statut du depot:" -ForegroundColor Yellow
git remote -v
git branch -a

Write-Host ""
Write-Host "Depot GitHub synchronise!" -ForegroundColor Green
Write-Host "Visitez: https://github.com/RahimeKABORE/CaptureSys" -ForegroundColor Cyan

Read-Host "Appuyez sur Entree pour fermer"
