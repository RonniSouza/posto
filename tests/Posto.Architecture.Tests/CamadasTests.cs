using NetArchTest.Rules;

namespace Posto.Architecture.Tests;

public class CamadasTests
{
    private const string ApplicationNs = "Posto.Application.";
    private const string InfrastructureNs = "Posto.Infrastructure.";
    private const string ApiNs = "Posto.Api.";
    private const string WorkerNs = "Posto.Worker.";

    [Fact]
    public void Domain_NaoDeveDependerDeOutrasCamadas()
    {
        var result = Types.InAssembly(typeof(Domain.AssemblyReference).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNs, InfrastructureNs, ApiNs, WorkerNs)
            .GetResult();

        Assert.True(result.IsSuccessful, Falhas(result));
    }

    [Fact]
    public void Application_NaoDeveDependerDeInfraestruturaNemApresentacao()
    {
        var result = Types.InAssembly(typeof(Application.AssemblyReference).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNs, ApiNs, WorkerNs)
            .GetResult();

        Assert.True(result.IsSuccessful, Falhas(result));
    }

    [Fact]
    public void Infrastructure_NaoDeveDependerDaApresentacao()
    {
        var result = Types.InAssembly(typeof(Infrastructure.AssemblyReference).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApiNs, WorkerNs)
            .GetResult();

        Assert.True(result.IsSuccessful, Falhas(result));
    }

    [Fact]
    public void Domain_NaoDeveDependerDePersistencia()
    {
        var result = Types.InAssembly(typeof(Domain.AssemblyReference).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Npgsql")
            .GetResult();

        Assert.True(result.IsSuccessful, Falhas(result));
    }

    [Fact]
    public void Application_NaoDeveDependerDeDetalhesDeInfraestrutura()
    {
        var result = Types.InAssembly(typeof(Application.AssemblyReference).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Npgsql", "CsvHelper", "System.Net.Http")
            .GetResult();

        Assert.True(result.IsSuccessful, Falhas(result));
    }

    [Fact]
    public void ErrosDoDominio_DevemSerClassesEstaticas()
    {
        var result = Types.InAssembly(typeof(Domain.AssemblyReference).Assembly)
            .That()
            .HaveNameEndingWith("Errors")
            .Should()
            .BeStatic()
            .GetResult();

        Assert.True(result.IsSuccessful, Falhas(result));
    }

    private static string Falhas(TestResult result) =>
        string.Join(", ", result.FailingTypeNames ?? []);
}
