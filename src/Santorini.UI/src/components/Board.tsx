import { DndContext } from '@dnd-kit/core'
import type { DragEndEvent } from '@dnd-kit/core'
import Cell from './Cell'
import Worker from './Worker'

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

interface BoardProps {
  gameState: GameState
  currentPlayer: string | null
  selectedWorker: { playerName: string, workerNumber: number, x: number, y: number } | null
  setSelectedWorker: (worker: { playerName: string, workerNumber: number, x: number, y: number } | null) => void
  pendingMove: { x: number, y: number } | null
  setPendingMove: (move: { x: number, y: number } | null) => void
  onMoveComplete: (moveToX: number, moveToY: number, buildAtX: number, buildAtY: number) => void
}

export default function Board({ 
  gameState, 
  currentPlayer, 
  selectedWorker, 
  setSelectedWorker,
  pendingMove,
  setPendingMove,
  onMoveComplete 
}: BoardProps) {

  const handleDragEnd = (event: DragEndEvent) => {
    const { over, active } = event
    if (over && active.data.current) {
      const overX = over.data.current?.x
      const overY = over.data.current?.y
      const { playerName, workerNumber } = active.data.current

      // Find original position of this worker
      const cell = gameState.board.find(c => c.workerOwner === playerName && c.workerNumber === workerNumber)
      if (cell) {
        setSelectedWorker({ playerName, workerNumber, x: cell.x, y: cell.y })
        setPendingMove({ x: overX, y: overY })
      }
    }
  }

  const handleCellClick = (x: number, y: number) => {
    if (pendingMove) {
      onMoveComplete(pendingMove.x, pendingMove.y, x, y)
    }
  }

  // Sort board by y then x to render in grid order
  const sortedBoard = [...gameState.board].sort((a, b) => {
    if (a.y !== b.y) return a.y - b.y
    return a.x - b.x
  })

  return (
    <DndContext onDragEnd={handleDragEnd}>
      <div className="board">
        {sortedBoard.map((cell) => {
          const isPendingMoveTarget = pendingMove?.x === cell.x && pendingMove?.y === cell.y
          const isWorkerHere = cell.hasWorker
          
          return (
            <Cell 
              key={`${cell.x}-${cell.y}`} 
              x={cell.x} 
              y={cell.y} 
              level={cell.level}
              onClick={() => handleCellClick(cell.x, cell.y)}
              isPendingMove={isPendingMoveTarget}
            >
              {isWorkerHere && (
                <Worker 
                  id={`worker-${cell.workerOwner}-${cell.workerNumber}`}
                  playerName={cell.workerOwner!}
                  workerNumber={cell.workerNumber!}
                  isDraggable={currentPlayer === cell.workerOwner && !gameState.gameOver && !pendingMove}
                />
              )}
              {isPendingMoveTarget && (
                <div className={`worker ${selectedWorker?.playerName === 'Player1' ? 'p1' : 'p2'} phantom`}>
                  {selectedWorker?.workerNumber}
                </div>
              )}
            </Cell>
          )
        })}
      </div>
    </DndContext>
  )
}
