using AsistenteIA.Models;
using AsistenteIA.Services;
using Xunit;

namespace AsistenteIA.Tests;

/// <summary>
/// Pruebas unitarias de integración local para DatabaseService con SQLite.
/// Cada prueba se ejecuta en un archivo de base de datos temporal aislado.
/// </summary>
public class DatabaseServiceTests : IAsyncLifetime
{
    private readonly string _tempDbPath;
    private readonly DatabaseService _databaseService;

    public DatabaseServiceTests()
    {
        _tempDbPath = Path.Combine(Path.GetTempPath(), $"asistente_test_{Guid.NewGuid():N}.db3");
        _databaseService = new DatabaseService(_tempDbPath);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        try
        {
            await _databaseService.CloseAsync();
            if (File.Exists(_tempDbPath))
            {
                File.Delete(_tempDbPath);
            }
        }
        catch
        {
            // Ignorar errores de limpieza de archivo temporal en el test runner
        }
    }

    [Fact]
    public async Task SaveConversationAsync_InsertsNewRecord_AssignsGeneratedId()
    {
        // Arrange
        var record = new ConversationRecord
        {
            Prompt = "¿Qué es una clave foránea?",
            Answer = "Una clave foránea es un campo que vincula dos tablas relacionales.",
            Category = "Bases de Datos",
            IsVerified = false
        };

        // Act
        int result = await _databaseService.SaveConversationAsync(record);
        var fetched = await _databaseService.GetConversationByIdAsync(record.Id);

        // Assert
        Assert.True(record.Id > 0);
        Assert.NotNull(fetched);
        Assert.Equal("¿Qué es una clave foránea?", fetched.Prompt);
        Assert.Equal("Bases de Datos", fetched.Category);
        Assert.False(fetched.IsVerified);
    }

    [Fact]
    public async Task GetConversationsAsync_ReturnsRecordsOrderedByCreatedAtDescending()
    {
        // Arrange
        var rec1 = new ConversationRecord
        {
            Prompt = "Pregunta antigua",
            Answer = "Respuesta 1",
            CreatedAt = DateTime.UtcNow.AddMinutes(-30)
        };
        var rec2 = new ConversationRecord
        {
            Prompt = "Pregunta intermedia",
            Answer = "Respuesta 2",
            CreatedAt = DateTime.UtcNow.AddMinutes(-15)
        };
        var rec3 = new ConversationRecord
        {
            Prompt = "Pregunta reciente",
            Answer = "Respuesta 3",
            CreatedAt = DateTime.UtcNow
        };

        await _databaseService.SaveConversationAsync(rec1);
        await _databaseService.SaveConversationAsync(rec2);
        await _databaseService.SaveConversationAsync(rec3);

        // Act
        var list = await _databaseService.GetConversationsAsync();

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal("Pregunta reciente", list[0].Prompt);
        Assert.Equal("Pregunta intermedia", list[1].Prompt);
        Assert.Equal("Pregunta antigua", list[2].Prompt);
    }

    [Fact]
    public async Task SearchConversationsAsync_FiltersByQueryInPromptOrAnswer()
    {
        // Arrange
        var rec1 = new ConversationRecord
        {
            Prompt = "¿Cómo funciona un árbol binario?",
            Answer = "Estructura de datos jerárquica.",
            Category = "Algoritmos"
        };
        var rec2 = new ConversationRecord
        {
            Prompt = "Optimizar consulta SQL",
            Answer = "Crear índices en columnas de búsqueda frecuentes.",
            Category = "Bases de Datos"
        };

        await _databaseService.SaveConversationAsync(rec1);
        await _databaseService.SaveConversationAsync(rec2);

        // Act: Búsqueda en prompt
        var searchPrompt = await _databaseService.SearchConversationsAsync("árbol");
        // Act: Búsqueda en respuesta
        var searchAnswer = await _databaseService.SearchConversationsAsync("índices");
        // Act: Búsqueda sin coincidencia
        var searchNone = await _databaseService.SearchConversationsAsync("inexistente");

        // Assert
        Assert.Single(searchPrompt);
        Assert.Equal(rec1.Id, searchPrompt[0].Id);

        Assert.Single(searchAnswer);
        Assert.Equal(rec2.Id, searchAnswer[0].Id);

        Assert.Empty(searchNone);
    }

    [Fact]
    public async Task SearchConversationsAsync_FiltersByCategory()
    {
        // Arrange
        var rec1 = new ConversationRecord { Prompt = "P1", Answer = "R1", Category = "Código" };
        var rec2 = new ConversationRecord { Prompt = "P2", Answer = "R2", Category = "Bases de Datos" };

        await _databaseService.SaveConversationAsync(rec1);
        await _databaseService.SaveConversationAsync(rec2);

        // Act
        var results = await _databaseService.SearchConversationsAsync(null, "Código");

        // Assert
        Assert.Single(results);
        Assert.Equal("Código", results[0].Category);
    }

    [Fact]
    public async Task UpdateConversationAsync_TogglesVerificationStatusSuccessfully()
    {
        // Arrange
        var record = new ConversationRecord
        {
            Prompt = "¿Qué es polimorfismo?",
            Answer = "Capacidad de un objeto de tomar diferentes formas.",
            Category = "Código",
            IsVerified = false
        };
        await _databaseService.SaveConversationAsync(record);

        // Act: Alternar verificación ética
        record.IsVerified = true;
        await _databaseService.UpdateConversationAsync(record);

        var updated = await _databaseService.GetConversationByIdAsync(record.Id);

        // Assert
        Assert.NotNull(updated);
        Assert.True(updated.IsVerified);
        Assert.False(updated.IsPendingVerification);
    }

    [Fact]
    public async Task DeleteConversationByIdAsync_RemovesRecordSuccessfully()
    {
        // Arrange
        var record = new ConversationRecord
        {
            Prompt = "Pregunta a eliminar",
            Answer = "Respuesta a eliminar"
        };
        await _databaseService.SaveConversationAsync(record);

        // Act
        int rowsDeleted = await _databaseService.DeleteConversationByIdAsync(record.Id);
        var fetched = await _databaseService.GetConversationByIdAsync(record.Id);

        // Assert
        Assert.Equal(1, rowsDeleted);
        Assert.Null(fetched);
    }
}
