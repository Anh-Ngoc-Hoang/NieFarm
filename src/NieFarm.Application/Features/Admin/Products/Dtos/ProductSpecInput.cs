namespace NieFarm.Application.Features.Admin.Products.Dtos;

/// <summary>One specification row as the admin form submits it, e.g. ("Xuất xứ", "Đắk Lắk").</summary>
public record ProductSpecInput(string Label, string Value);
