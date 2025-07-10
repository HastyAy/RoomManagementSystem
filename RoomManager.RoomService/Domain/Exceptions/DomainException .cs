namespace RoomManager.RoomService.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
        protected DomainException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class RoomNotFoundException : DomainException
    {
        public RoomNotFoundException(Guid roomId)
            : base($"Room with ID {roomId} was not found") { }
    }

    public class RoomNameAlreadyExistsException : DomainException
    {
        public RoomNameAlreadyExistsException(string name)
            : base($"Room with name '{name}' already exists") { }
    }

    public class InvalidRoomDataException : DomainException
    {
        public InvalidRoomDataException(string message) : base(message) { }
    }
}