import { useDroppable } from '@dnd-kit/core'

interface CellProps {
  x: number
  y: number
  level: number
  children?: React.ReactNode
  onClick?: () => void
  isPendingMove?: boolean
}

export default function Cell({ x, y, level, children, onClick, isPendingMove }: CellProps) {
  const { isOver, setNodeRef } = useDroppable({
    id: `cell-${x}-${y}`,
    data: { x, y }
  })

  const levelClass = `level-${level}`
  const pendingClass = isPendingMove ? 'pending-move' : ''

  return (
    <div 
      ref={setNodeRef}
      className={`cell ${levelClass} ${isOver ? 'over' : ''} ${pendingClass}`}
      onClick={onClick}
    >
      {children}
      {level > 0 && level < 4 && <div className="level-indicator">{level}</div>}
    </div>
  )
}
