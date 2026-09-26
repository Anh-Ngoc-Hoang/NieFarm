namespace NieFarm.Application.Features.Admin.Products.Dtos;

/// <summary>One option axis as the admin form submits it, e.g. ("Đơn vị", ["250g", "500g"]).</summary>
public record ProductOptionInput(string Name, List<string> Values);
