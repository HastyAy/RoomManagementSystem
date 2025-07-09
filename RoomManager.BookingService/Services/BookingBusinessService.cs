using RoomManager.BookingService.Entities;
using RoomManager.BookingService.Repositories;
using RoomManager.Shared.Common;
using RoomManager.Shared.DTOs.BookingDto;
using RoomManager.Shared.DTOs.UserDto;

namespace RoomManager.BookingService.Services
{
    public class BookingBusinessService : IBookingBusinessService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomServiceClient _roomServiceClient;
        private readonly IUserServiceClient _userServiceClient;
        private readonly ILogger<BookingBusinessService> _logger;

        public BookingBusinessService(
            IBookingRepository bookingRepository,
            IRoomServiceClient roomServiceClient,
            IUserServiceClient userServiceClient,
            ILogger<BookingBusinessService> logger)
        {
            _bookingRepository = bookingRepository;
            _roomServiceClient = roomServiceClient;
            _userServiceClient = userServiceClient;
            _logger = logger;
        }

        public async Task<ServiceResponse<BookingDto>> CreateBookingAsync(CreateBookingRequest request)
        {
            try
            {
                // Validate request
                var validationResult = await ValidateBookingRequestAsync(request);
                if (!validationResult.Success)
                {
                    return ServiceResponse<BookingDto>.ErrorResponse(validationResult.Message!);
                }

                // Check room availability
                if (!await _bookingRepository.IsRoomAvailableAsync(request.RoomId, request.StartTime, request.EndTime))
                {
                    return ServiceResponse<BookingDto>.ErrorResponse("Room is not available for the specified time slot");
                }

                // Check user availability
                if (await _bookingRepository.HasUserConflictAsync(request.StudentId, request.ProfessorId, request.StartTime, request.EndTime))
                {
                    return ServiceResponse<BookingDto>.ErrorResponse("User already has a booking during this time slot");
                }

                // Get additional data from other services
                var roomData = await _roomServiceClient.GetRoomAsync(request.RoomId);
                StudentDto? studentData = null;
                ProfessorDto? professorData = null;

                if (request.StudentId.HasValue)
                {
                    studentData = await _userServiceClient.GetStudentAsync(request.StudentId.Value);
                }

                if (request.ProfessorId.HasValue)
                {
                    professorData = await _userServiceClient.GetProfessorAsync(request.ProfessorId.Value);
                }

                // Create booking entity
                var booking = new Booking
                {
                    RoomId = request.RoomId,
                    StudentId = request.StudentId,
                    ProfessorId = request.ProfessorId,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Purpose = request.Purpose,

                    // Denormalized data
                    RoomName = roomData?.Name,
                    RoomLocation = roomData?.Location,
                    RoomCapacity = roomData?.Capacity,
                    StudentName = studentData?.FullName,
                    StudentMatriNumber = studentData?.MatriNumber,
                    ProfessorName = professorData?.FullName,
                    ProfessorDepartment = professorData?.Department
                };

                await _bookingRepository.AddAsync(booking);

                var bookingDto = MapToDto(booking);
                return ServiceResponse<BookingDto>.SuccessResponse(bookingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return ServiceResponse<BookingDto>.ErrorResponse("Failed to create booking");
            }
        }

        public async Task<ServiceResponse<BookingDto>> UpdateBookingAsync(Guid id, UpdateBookingRequest request)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(id);
                if (booking == null)
                {
                    return ServiceResponse<BookingDto>.ErrorResponse("Booking not found");
                }

                // Validate request
                var validationResult = await ValidateBookingRequestAsync(new CreateBookingRequest
                {
                    RoomId = request.RoomId,
                    StudentId = request.StudentId,
                    ProfessorId = request.ProfessorId,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Purpose = request.Purpose
                });

                if (!validationResult.Success)
                {
                    return ServiceResponse<BookingDto>.ErrorResponse(validationResult.Message!);
                }

                // Check room availability (excluding current booking)
                if (!await _bookingRepository.IsRoomAvailableAsync(request.RoomId, request.StartTime, request.EndTime, id))
                {
                    return ServiceResponse<BookingDto>.ErrorResponse("Room is not available for the specified time slot");
                }

                // Check user availability (excluding current booking)
                if (await _bookingRepository.HasUserConflictAsync(request.StudentId, request.ProfessorId, request.StartTime, request.EndTime, id))
                {
                    return ServiceResponse<BookingDto>.ErrorResponse("User already has a booking during this time slot");
                }

                // Update booking
                booking.RoomId = request.RoomId;
                booking.StudentId = request.StudentId;
                booking.ProfessorId = request.ProfessorId;
                booking.StartTime = request.StartTime;
                booking.EndTime = request.EndTime;
                booking.Purpose = request.Purpose;

                // Update denormalized data if room or user changed
                if (booking.RoomId != request.RoomId)
                {
                    var roomData = await _roomServiceClient.GetRoomAsync(request.RoomId);
                    booking.RoomName = roomData?.Name;
                    booking.RoomLocation = roomData?.Location;
                    booking.RoomCapacity = roomData?.Capacity;
                }

                // Update user data if changed
                if (booking.StudentId != request.StudentId || booking.ProfessorId != request.ProfessorId)
                {
                    booking.StudentName = null;
                    booking.StudentMatriNumber = null;
                    booking.ProfessorName = null;
                    booking.ProfessorDepartment = null;

                    if (request.StudentId.HasValue)
                    {
                        var studentData = await _userServiceClient.GetStudentAsync(request.StudentId.Value);
                        booking.StudentName = studentData?.FullName;
                        booking.StudentMatriNumber = studentData?.MatriNumber;
                    }

                    if (request.ProfessorId.HasValue)
                    {
                        var professorData = await _userServiceClient.GetProfessorAsync(request.ProfessorId.Value);
                        booking.ProfessorName = professorData?.FullName;
                        booking.ProfessorDepartment = professorData?.Department;
                    }
                }

                await _bookingRepository.UpdateAsync(booking);

                var bookingDto = MapToDto(booking);
                return ServiceResponse<BookingDto>.SuccessResponse(bookingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking {BookingId}", id);
                return ServiceResponse<BookingDto>.ErrorResponse("Failed to update booking");
            }
        }

        public async Task<ServiceResponse<object>> DeleteBookingAsync(Guid id)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(id);
                if (booking == null)
                {
                    return ServiceResponse<object>.ErrorResponse("Booking not found");
                }

                await _bookingRepository.DeleteAsync(id);
                return ServiceResponse<object>.SuccessResponse(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking {BookingId}", id);
                return ServiceResponse<object>.ErrorResponse("Failed to delete booking");
            }
        }

        public async Task<ServiceResponse<BookingDto>> GetBookingAsync(Guid id)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(id);
                if (booking == null)
                {
                    return ServiceResponse<BookingDto>.ErrorResponse("Booking not found");
                }

                var bookingDto = MapToDto(booking);
                return ServiceResponse<BookingDto>.SuccessResponse(bookingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingId}", id);
                return ServiceResponse<BookingDto>.ErrorResponse("Failed to get booking");
            }
        }

        public async Task<ServiceResponse<List<BookingDto>>> GetAllBookingsAsync()
        {
            try
            {
                var bookings = await _bookingRepository.GetActiveBookingsAsync();
                var bookingDtos = bookings.Select(MapToDto).ToList();
                return ServiceResponse<List<BookingDto>>.SuccessResponse(bookingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all bookings");
                return ServiceResponse<List<BookingDto>>.ErrorResponse("Failed to get bookings");
            }
        }

        public async Task<ServiceResponse<List<BookingDto>>> GetBookingsByRoomAsync(Guid roomId)
        {
            try
            {
                var bookings = await _bookingRepository.GetByRoomIdAsync(roomId);
                var bookingDtos = bookings.Select(MapToDto).ToList();
                return ServiceResponse<List<BookingDto>>.SuccessResponse(bookingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for room {RoomId}", roomId);
                return ServiceResponse<List<BookingDto>>.ErrorResponse("Failed to get bookings");
            }
        }

        public async Task<ServiceResponse<List<BookingDto>>> GetBookingsByUserAsync(Guid? studentId, Guid? professorId)
        {
            try
            {
                List<Booking> bookings;

                if (studentId.HasValue)
                {
                    bookings = await _bookingRepository.GetByStudentIdAsync(studentId.Value);
                }
                else if (professorId.HasValue)
                {
                    bookings = await _bookingRepository.GetByProfessorIdAsync(professorId.Value);
                }
                else
                {
                    return ServiceResponse<List<BookingDto>>.ErrorResponse("Either StudentId or ProfessorId must be provided");
                }

                var bookingDtos = bookings.Select(MapToDto).ToList();
                return ServiceResponse<List<BookingDto>>.SuccessResponse(bookingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for user. StudentId: {StudentId}, ProfessorId: {ProfessorId}", studentId, professorId);
                return ServiceResponse<List<BookingDto>>.ErrorResponse("Failed to get bookings");
            }
        }

        public async Task<ServiceResponse<List<BookingDto>>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                {
                    return ServiceResponse<List<BookingDto>>.ErrorResponse("Start date must be before end date");
                }

                var bookings = await _bookingRepository.GetByDateRangeAsync(startDate, endDate);
                var bookingDtos = bookings.Select(MapToDto).ToList();
                return ServiceResponse<List<BookingDto>>.SuccessResponse(bookingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for date range {StartDate} - {EndDate}", startDate, endDate);
                return ServiceResponse<List<BookingDto>>.ErrorResponse("Failed to get bookings");
            }
        }

        public async Task<ServiceResponse<bool>> CheckRoomAvailabilityAsync(Guid roomId, DateTime startTime, DateTime endTime)
        {
            try
            {
                // Check if room exists
                if (!await _roomServiceClient.RoomExistsAsync(roomId))
                {
                    return ServiceResponse<bool>.ErrorResponse("Room not found");
                }

                var isAvailable = await _bookingRepository.IsRoomAvailableAsync(roomId, startTime, endTime);
                return ServiceResponse<bool>.SuccessResponse(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking room availability for room {RoomId}", roomId);
                return ServiceResponse<bool>.ErrorResponse("Failed to check room availability");
            }
        }

        private async Task<ServiceResponse<object>> ValidateBookingRequestAsync(CreateBookingRequest request)
        {
            // Basic validation
            if (request.StartTime >= request.EndTime)
            {
                return ServiceResponse<object>.ErrorResponse("End time must be after start time");
            }

            if (request.StartTime <= DateTime.UtcNow.AddMinutes(-5)) // Allow 5 minutes tolerance
            {
                return ServiceResponse<object>.ErrorResponse("Cannot create bookings in the past");
            }

            if (!request.StudentId.HasValue && !request.ProfessorId.HasValue)
            {
                return ServiceResponse<object>.ErrorResponse("Either StudentId or ProfessorId must be provided");
            }

            if (request.StudentId.HasValue && request.ProfessorId.HasValue)
            {
                return ServiceResponse<object>.ErrorResponse("Cannot specify both StudentId and ProfessorId");
            }

            // Check if room exists
            if (!await _roomServiceClient.RoomExistsAsync(request.RoomId))
            {
                return ServiceResponse<object>.ErrorResponse("Room not found");
            }

            // Check if user exists
            if (request.StudentId.HasValue && !await _userServiceClient.StudentExistsAsync(request.StudentId.Value))
            {
                return ServiceResponse<object>.ErrorResponse("Student not found");
            }

            if (request.ProfessorId.HasValue && !await _userServiceClient.ProfessorExistsAsync(request.ProfessorId.Value))
            {
                return ServiceResponse<object>.ErrorResponse("Professor not found");
            }

            return ServiceResponse<object>.SuccessResponse(null);
        }

        private static BookingDto MapToDto(Booking booking)
        {
            return new BookingDto
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                RoomName = booking.RoomName,
                RoomLocation = booking.RoomLocation,
                StudentId = booking.StudentId,
                StudentName = booking.StudentName,
                ProfessorId = booking.ProfessorId,
                ProfessorName = booking.ProfessorName,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Purpose = booking.Purpose,
                CreatedAt = booking.CreatedAt
            };
        }
    }
}