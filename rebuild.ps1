# Script para limpar e reconstruir o projeto
Write-Host "🧹 Limpando projeto..." -ForegroundColor Yellow

# Limpar diretórios bin e obj
Remove-Item -Path ".\ChemicalSimulator\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ".\ChemicalSimulator\obj" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "✅ Diretórios bin e obj removidos" -ForegroundColor Green

# Restaurar pacotes
Write-Host "📦 Restaurando pacotes NuGet..." -ForegroundColor Yellow
dotnet restore

# Compilar o projeto
Write-Host "🔨 Compilando projeto..." -ForegroundColor Yellow
dotnet build --no-restore

Write-Host "✅ Build concluído!" -ForegroundColor Green
Write-Host ""
Write-Host "Agora feche e reabra o Visual Studio para limpar o cache do designer XAML." -ForegroundColor Cyan
