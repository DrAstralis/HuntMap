using System;
using System.Collections.Generic;

namespace HuntMap.Domain;

public record PinDto(Guid Id, string Name, int Tier, int Quantity, string Color, PinSymbol Symbol, double X, double Y, Guid OwnerId);
public record CreatePinRequest(string Name, int Tier, int Quantity, double X, double Y);
public record UpdatePinRequest(string Name, int Tier, int Quantity, double X, double Y);
public record ShareInviteRequest(string Email);
public record ShareDecisionRequest(Guid ShareId, string Decision); // accept/reject/block

public record MapSettingsDto(string ImagePath, int Width, int Height, IReadOnlyDictionary<int, string> TierColors);