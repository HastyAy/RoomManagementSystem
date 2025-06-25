using Microsoft.EntityFrameworkCore;
using RoomManager.Shared.Entities;
using RoomManager.Shared.Repositories;
using RoomManager.BookingService.Context;

namespace RoomManager.BookingService.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly BookingDbContext _context;

    public StudentRepository(BookingDbContext context) => _context = context;

    public async Task<List<Student>> GetAllAsync() => await _context.Students.ToListAsync();

    public async Task<Student?> GetByIdAsync(Guid id) => await _context.Students.FindAsync(id);

    public async Task AddAsync(Student student)
    {
        student.Id = Guid.NewGuid();
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student != null)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
    }
}
