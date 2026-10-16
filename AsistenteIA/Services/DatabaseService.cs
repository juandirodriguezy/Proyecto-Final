using SQLite;
using AsistenteIA.Models;

namespace AsistenteIA.Services;

/// <summary>
/// Servicio de persistencia local mediante SQLite con soporte para operaciones CRUD asíncronas.
/// </summary>
public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly string _databasePath;

    public DatabaseService()
    {
        _databasePath = Path.Combine(FileSystem.AppDataDirectory, "asistente.db3");
    }

    /// <summary>
    /// Retorna la ruta física del archivo de base de datos local SQLite.
    /// </summary>
    public string GetDatabasePath() => _databasePath;

    /// <summary>
    /// Inicializa la conexión y crea las tablas requeridas si aún no existen.
    /// </summary>
    public async Task InitAsync()
    {
        if (_database is not null)
            return;

        await _semaphore.WaitAsync();
        try
        {
            if (_database is null)
            {
                _database = new SQLiteAsyncConnection(_databasePath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);
                await _database.CreateTableAsync<ConversationRecord>();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Obtiene todas las conversaciones almacenadas, ordenadas de la más reciente a la más antigua.
    /// </summary>
    public async Task<List<ConversationRecord>> GetConversationsAsync()
    {
        await InitAsync();
        return await _database!.Table<ConversationRecord>()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene una conversación por su identificador primario.
    /// </summary>
    public async Task<ConversationRecord?> GetConversationByIdAsync(int id)
    {
        await InitAsync();
        return await _database!.Table<ConversationRecord>()
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Busca y filtra conversaciones por texto contenido en el prompt o la respuesta, y opcionalmente por categoría.
    /// </summary>
    public async Task<List<ConversationRecord>> SearchConversationsAsync(string? query, string? category = null)
    {
        await InitAsync();
        var all = await _database!.Table<ConversationRecord>()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        IEnumerable<ConversationRecord> filtered = all;

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("Todas", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            string cleanQuery = query.Trim();
            filtered = filtered.Where(c =>
                c.Prompt.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase) ||
                c.Answer.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase));
        }

        return filtered.ToList();
    }

    /// <summary>
    /// Inserta una nueva conversación o actualiza una existente si ya tiene Id asignado.
    /// </summary>
    public async Task<int> SaveConversationAsync(ConversationRecord record)
    {
        await InitAsync();
        if (record.Id != 0)
        {
            await _database!.UpdateAsync(record);
            return record.Id;
        }

        return await _database!.InsertAsync(record);
    }

    /// <summary>
    /// Actualiza el estado de una conversación existente (por ejemplo, para marcarla como verificada).
    /// </summary>
    public async Task<int> UpdateConversationAsync(ConversationRecord record)
    {
        await InitAsync();
        return await _database!.UpdateAsync(record);
    }

    /// <summary>
    /// Elimina una conversación de la base de datos.
    /// </summary>
    public async Task<int> DeleteConversationAsync(ConversationRecord record)
    {
        await InitAsync();
        return await _database!.DeleteAsync(record);
    }

    /// <summary>
    /// Elimina una conversación a partir de su ID.
    /// </summary>
    public async Task<int> DeleteConversationByIdAsync(int id)
    {
        await InitAsync();
        return await _database!.DeleteAsync<ConversationRecord>(id);
    }
}
