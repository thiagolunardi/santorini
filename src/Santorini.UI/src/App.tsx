import { useState, useEffect, useCallback } from 'react'
import axios from 'axios'
import './App.css'
import Board from './components/Board'

interface GameState {
  board: Array<{
    x: number
    y: number
    level: number
    hasWorker: boolean
    workerOwner: string | null
    workerNumber: number | null
  }>
  players: string[]
  winner: string | null
  gameOver: boolean
}

interface AvailableMove {
  workerNumber: number
  moveTo: { x: number, y: number }
  buildAt: { x: number, y: number }
}

function App() {
  const [gameState, setGameState] = useState<GameState | null>(null)
  const [currentPlayer, setCurrentPlayer] = useState<string | null>(null)
  const [availableMoves, setAvailableMoves] = useState<AvailableMove[]>([])
  const [selectedWorker, setSelectedWorker] = useState<{ playerName: string, workerNumber: number, x: number, y: number } | null>(null)
  const [pendingMove, setPendingMove] = useState<{ x: number, y: number } | null>(null)

  const fetchState = useCallback(async () => {
    try {
      const stateRes = await axios.get('/game/state')
      setGameState(stateRes.data)
      const turnRes = await axios.get('/game/turn')
      setCurrentPlayer(turnRes.data.currentPlayer)
      setAvailableMoves(turnRes.data.availableMoves || [])
    } catch (error) {
      console.error('Error fetching game state:', error)
    }
  }, [])

  useEffect(() => {
    const timeout = setTimeout(fetchState, 0)
    const interval = setInterval(fetchState, 1000)
    return () => {
      clearTimeout(timeout)
      clearInterval(interval)
    }
  }, [fetchState])

  const handleMove = async (moveToX: number, moveToY: number, buildAtX: number, buildAtY: number) => {
    if (!selectedWorker) return

    try {
      await axios.post('/game/move', {
        playerName: selectedWorker.playerName,
        workerNumber: selectedWorker.workerNumber,
        moveToX,
        moveToY,
        buildAtX,
        buildAtY
      })
      setSelectedWorker(null)
      setPendingMove(null)
      fetchState()
    } catch (error) {
      console.error('Error making move:', error)
      alert('Invalid move!')
      setSelectedWorker(null)
      setPendingMove(null)
    }
  }

  const resetGame = async () => {
    try {
      await axios.post('/game/reset')
      fetchState()
    } catch (error) {
      console.error('Error resetting game:', error)
    }
  }

  if (!gameState) return <div>Loading...</div>

  return (
    <div className="game-container">
      <h1>Santorini</h1>
      <div className="game-info">
        <p>Current Player: <span className={currentPlayer === 'Player1' ? 'p1' : 'p2'}>{currentPlayer}</span></p>
        {gameState.gameOver && <p className="winner">Winner: {gameState.winner}!</p>}
        <button onClick={resetGame}>Reset Game</button>
      </div>
      
      <Board 
        gameState={gameState} 
        currentPlayer={currentPlayer}
        availableMoves={availableMoves}
        selectedWorker={selectedWorker}
        setSelectedWorker={setSelectedWorker}
        pendingMove={pendingMove}
        setPendingMove={setPendingMove}
        onMoveComplete={handleMove}
      />

      <div className="instructions">
        {selectedWorker && !pendingMove && <p>Select where to move the worker (Drag and Drop)</p>}
        {pendingMove && <p>Click an adjacent square to build!</p>}
      </div>
    </div>
  )
}

export default App
