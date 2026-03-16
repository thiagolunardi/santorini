[![CI](https://github.com/thiagolunardi/santorini/actions/workflows/build.yml/badge.svg)](https://github.com/thiagolunardi/santorini/actions/workflows/build.yml)
[![codecov](https://codecov.io/gh/thiagolunardi/santorini/graph/badge.svg?token=8AD0163MVG)](https://codecov.io/gh/thiagolunardi/santorini)

# Santorini

A digital implementation of the **Santorini** board game — built as a platform for experimenting with multi-AI agent
architectures.

---

## About the Game

Santorini is a strategic 2-player (or more, with god powers) board game set on the sun-drenched island of Santorini,
Greece. Players take turns moving their workers across a 5×5 grid and constructing building levels on adjacent spaces.
The first player to move one of their workers up to the **third level** of a tower wins.

Simple rules, deep strategy.

[![Santorini — Official Rules](https://img.youtube.com/vi/zw0BDEjO5KI/0.jpg)](https://www.youtube.com/watch?v=zw0BDEjO5KI)

---

## About This Project

This project is a learning playground for building a **multi-AI agent architecture**. The goal is to have autonomous AI
agents play Santorini against each other, making decisions through a structured game API.

The game engine exposes itself as an **MCP (Model Context Protocol) server**, allowing AI agents to connect, perceive
the game state, and issue moves using standard tooling. The architecture is built with:

- **.NET 10** — game engine and API
- **.NET Aspire** — orchestration of services (API + UI)
- **Model Context Protocol (MCP)** — interface for AI agents to interact with the game

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for the UI, optional)

### Clone

```bash
git clone https://github.com/thiagolunardi/santorini.git
cd santorini
```

### Build

```bash
dotnet build
```

### Run

#### Option A — Full stack with .NET Aspire (Recommended)

The `Santorini.AppHost` project orchestrates both the API and the UI together:

```bash
dotnet run --project src/Santorini.AppHost
```

This starts the [.NET Aspire](https://learn.microsoft.com/dotnet/aspire) dashboard (typically at `https://localhost:15888`), which manages the game API and the Vite UI automatically. Node.js is required for the UI to start.

#### Option B — Individual components

**API only** (useful for connecting AI agents via MCP):

```bash
dotnet run --project src/Santorini.Host
```

- REST API and MCP endpoint available at `/mcp`
- OpenAPI docs available at `/openapi/v1.json` (Development mode)

**UI only** (in a separate terminal):

```bash
cd src/Santorini.UI
npm install
npm run dev
```

> Set the `VITE_API_URL` environment variable to point to the API URL when not using Aspire.

### Run Tests

```bash
dotnet test
```

---

## Project Structure

```
src/
  Santorini/              # Core game engine (board, pieces, rules)
  Santorini.Host/         # ASP.NET Core API + MCP server endpoint (/mcp)
  Santorini.AppHost/      # .NET Aspire orchestration host
  Santorini.ServiceDefaults/  # Shared Aspire service defaults
  Santorini.UI/           # Front-end UI (Vite)
tests/
  Santorini.Tests/        # Unit and integration tests
```
