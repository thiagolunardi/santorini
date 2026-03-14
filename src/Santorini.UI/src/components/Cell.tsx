import { useDroppable } from '@dnd-kit/core'

interface CellProps {
  x: number
  y: number
  level: number
  children?: React.ReactNode
  onClick?: () => void
  isPendingMove?: boolean
  isHighlight?: boolean
}

export default function Cell({ x, y, level, children, onClick, isPendingMove, isHighlight }: CellProps) {
  const { isOver, setNodeRef } = useDroppable({
    id: `cell-${x}-${y}`,
    data: { x, y }
  })

  const levelClass = `level-${level}`
  const pendingClass = isPendingMove ? 'pending-move' : ''
  const highlightClass = isHighlight ? 'highlight' : ''

  return (
    <div 
      ref={setNodeRef}
      className={`cell ${levelClass} ${isOver ? 'over' : ''} ${pendingClass} ${highlightClass}`}
      onClick={onClick}
    >
      {children}
      {level > 0 && level < 4 && <div className="level-indicator">{level}</div>}
    </div>
  )
}
