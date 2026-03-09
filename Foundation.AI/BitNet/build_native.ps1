# ============================================================================
# BitNet Native Build Script for Windows x86_64
# ============================================================================
# Prerequisites: CMake, Clang (via Visual Studio Build Tools), Python 3
#
# Usage:
#   .\build_native.ps1
#   .\build_native.ps1 -Model "microsoft/BitNet-b1.58-2B-4T"
#   .\build_native.ps1 -SkipModel
# ============================================================================

param(
    [string]$Model = "microsoft/BitNet-b1.58-2B-4T",
    [string]$QuantType = "i2_s",
    [switch]$SkipModel,
    [switch]$Clean
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
# If called from csharp/, go up one level. If called from repo root, stay.
if (Test-Path (Join-Path $RepoRoot "CMakeLists.txt")) {
    # Already at repo root
} else {
    $RepoRoot = Split-Path -Parent $RepoRoot
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  BitNet Native Build (Windows x86_64)"    -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

Push-Location $RepoRoot
try {
    # ── Clean ────────────────────────────────────────────────────
    if ($Clean -and (Test-Path "build")) {
        Write-Host "[1/4] Cleaning previous build..." -ForegroundColor Yellow
        Remove-Item -Recurse -Force "build"
    }

    # ── Code Generation ──────────────────────────────────────────
    Write-Host "[1/4] Running code generation (tl2 for x86_64)..." -ForegroundColor Yellow
    $codegenModel = switch ($Model) {
        "microsoft/BitNet-b1.58-2B-4T" { "bitnet_b1_58-3B" }
        "1bitLLM/bitnet_b1_58-3B"      { "bitnet_b1_58-3B" }
        "1bitLLM/bitnet_b1_58-large"   { "bitnet_b1_58-large" }
        default                         { "bitnet_b1_58-3B" }
    }

    $codegenArgs = switch ($codegenModel) {
        "bitnet_b1_58-3B"    { @("--model", "bitnet_b1_58-3B", "--BM", "160,320,320", "--BK", "96,96,96", "--bm", "32,32,32") }
        "bitnet_b1_58-large" { @("--model", "bitnet_b1_58-large", "--BM", "256,128,256", "--BK", "96,192,96", "--bm", "32,32,32") }
    }

    python utils/codegen_tl2.py @codegenArgs
    if ($LASTEXITCODE -ne 0) { throw "Code generation failed" }

    # ── CMAKE Configure ──────────────────────────────────────────
    Write-Host "[2/4] Configuring CMake..." -ForegroundColor Yellow
    $cmakeArgs = @(
        "-B", "build",
        "-DBITNET_X86_TL2=OFF",
        "-T", "ClangCL",
        "-DCMAKE_C_COMPILER=clang",
        "-DCMAKE_CXX_COMPILER=clang++",
        "-DBUILD_SHARED_LIBS=ON"
    )
    cmake @cmakeArgs
    if ($LASTEXITCODE -ne 0) { throw "CMake configure failed" }

    # ── Build ─────────────────────────────────────────────────────
    Write-Host "[3/4] Building native libraries (Release)..." -ForegroundColor Yellow
    cmake --build build --config Release
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }

    # ── Copy native DLLs to runtimes folder ───────────────────────
    Write-Host "[4/4] Copying native binaries..." -ForegroundColor Yellow
    $runtimesDir = Join-Path $RepoRoot "csharp" "runtimes" "win-x64" "native"
    New-Item -ItemType Directory -Force -Path $runtimesDir | Out-Null

    $binDir = Join-Path $RepoRoot "build" "bin" "Release"
    $dllsToCopy = @("llama.dll", "ggml.dll", "ggml-base.dll", "ggml-cpu.dll")

    foreach ($dll in $dllsToCopy) {
        $src = Join-Path $binDir $dll
        if (Test-Path $src) {
            Copy-Item $src $runtimesDir -Force
            Write-Host "  Copied $dll" -ForegroundColor Green
        } else {
            # Some builds may name things differently, search for it
            $found = Get-ChildItem -Path $binDir -Filter $dll -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($found) {
                Copy-Item $found.FullName $runtimesDir -Force
                Write-Host "  Copied $dll (from $($found.DirectoryName))" -ForegroundColor Green
            } else {
                Write-Host "  Warning: $dll not found in build output" -ForegroundColor DarkYellow
            }
        }
    }

    # Also copy any other DLLs that might be needed
    Get-ChildItem -Path $binDir -Filter "*.dll" | ForEach-Object {
        if ($_.Name -notin $dllsToCopy) {
            Copy-Item $_.FullName $runtimesDir -Force
            Write-Host "  Copied $($_.Name) (extra)" -ForegroundColor DarkGray
        }
    }

    # ── Model download (optional) ─────────────────────────────────
    if (-not $SkipModel) {
        Write-Host ""
        Write-Host "To download and prepare the model, run:" -ForegroundColor Cyan
        Write-Host "  python setup_env.py --hf-repo $Model -q $QuantType" -ForegroundColor White
        Write-Host ""
    }

    Write-Host ""
    Write-Host "Build complete!" -ForegroundColor Green
    Write-Host "Native DLLs: $runtimesDir" -ForegroundColor Green
    Write-Host ""
}
finally {
    Pop-Location
}
