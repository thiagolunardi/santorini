import { useDraggable } from '@dnd-kit/core'
import { CSS } from '@dnd-kit/utilities'

interface WorkerProps {
  id: string
  playerName: string
  workerNumber: number
  isDraggable: boolean
}

export default function Worker({ id, playerName, workerNumber, isDraggable }: WorkerProps) {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id,
    disabled: !isDraggable,
    data: {
      playerName,
      workerNumber
    }
  })

  const style = {
    transform: CSS.Translate.toString(transform),
  }

  return (
    <div
      ref={setNodeRef}
      style={style}
      {...listeners}
      {...attributes}
      className={`worker ${playerName === 'Player1' ? 'p1' : 'p2'} ${isDragging ? 'dragging' : ''}`}
    >
      {workerNumber}
    </div>
  )
}
