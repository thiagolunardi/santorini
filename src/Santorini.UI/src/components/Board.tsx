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

interface AvailableMove {
  workerNumber: number
  moveTo: { x: number, y: number }
  buildAt: { x: number, y: number }
}

interface BoardProps {
  gameState: GameState
  currentPlayer: string | null
  availableMoves: AvailableMove[]
  selectedWorker: { playerName: string, workerNumber: number, x: number, y: number } | null
  setSelectedWorker: (worker: { playerName: string, workerNumber: number, x: number, y: number } | null) => void
  pendingMove: { x: number, y: number } | null
  setPendingMove: (move: { x: number, y: number } | null) => void
  onMoveComplete: (moveToX: number, moveToY: number, buildAtX: number, buildAtY: number) => void
}

export default function Board({ 
  gameState, 
  currentPlayer, 
  availableMoves,
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

      // Validate if this move is allowed for this worker
      const isValidMove = availableMoves.some(m => 
        m.workerNumber === workerNumber && 
        m.moveTo.x === overX && 
        m.moveTo.y === overY
      )

      if (isValidMove) {
        const cell = gameState.board.find(c => c.workerOwner === playerName && c.workerNumber === workerNumber)
        if (cell) {
          setSelectedWorker({ playerName, workerNumber, x: cell.x, y: cell.y })
          setPendingMove({ x: overX, y: overY })
        }
      }
    }
  }

  const handleCellClick = (x: number, y: number) => {
    if (pendingMove && selectedWorker) {
      // Validate if building here is allowed for the pending move
      const isValidBuild = availableMoves.some(m => 
        m.workerNumber === selectedWorker.workerNumber && 
        m.moveTo.x === pendingMove.x && 
        m.moveTo.y === pendingMove.y &&
        m.buildAt.x === x &&
        m.buildAt.y === y
      )

      if (isValidBuild) {
        onMoveComplete(pendingMove.x, pendingMove.y, x, y)
      }
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
          
          let isHighlight = false
          if (pendingMove && selectedWorker) {
            // Highlight possible build locations
            isHighlight = availableMoves.some(m => 
              m.workerNumber === selectedWorker.workerNumber && 
              m.moveTo.x === pendingMove.x && 
              m.moveTo.y === pendingMove.y &&
              m.buildAt.x === cell.x &&
              m.buildAt.y === cell.y
            )
          } else {
            // Highlight possible move locations for ANY worker of the current player
            isHighlight = availableMoves.some(m => 
              m.moveTo.x === cell.x && 
              m.moveTo.y === cell.y
            )
          }
          
          return (
            <Cell 
              key={`${cell.x}-${cell.y}`} 
              x={cell.x} 
              y={cell.y} 
              level={cell.level}
              onClick={() => handleCellClick(cell.x, cell.y)}
              isPendingMove={isPendingMoveTarget}
              isHighlight={isHighlight}
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
