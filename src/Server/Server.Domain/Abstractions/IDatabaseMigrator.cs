namespace Server.Domain.Abstractions;

public interface IDatabaseMigrator
{
    void Migrate();
}
