var builder = DistributedApplication.CreateBuilder(args);

var cryptoService = builder.AddProject<Projects.CryptoService>("cryptoservice");

builder.AddProject<Projects.CryptoCheck_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cryptoService)
    .WaitFor(cryptoService);

builder.Build().Run();
