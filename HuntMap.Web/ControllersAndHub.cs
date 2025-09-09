using System;
using System.Linq;
using System.Threading.Tasks;
using HuntMap.Data;
using HuntMap.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HuntMap.Web;

[ApiController]
[Route("api/[controller]")]
public class PinsController : ControllerBase
{
    private readonly HuntMapContext _db;
    private readonly UserManager<ApplicationUser> _um;
    private readonly IHubContext<PinHub, IPinClient> _hub;

    public PinsController(HuntMapContext db, UserManager<ApplicationUser> um, IHubContext<PinHub, IPinClient> hub)
    {
        _db = db; _um = um; _hub = hub;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int[]? tiers = null)
    {
        var userId = GetUserIdOrDefault();
        var acceptedSharesFrom = userId == Guid.Empty
            ? new List<Guid>() // make both branches List<Guid> to avoid CS0173
            : await _db.Shares.Where(s => s.RecipientId == userId && s.Status == ShareStatus.Accepted)
                              .Select(s => s.OwnerId).ToListAsync();

        var q = _db.Pins.AsNoTracking()
            .Where(p => !p.IsDeleted && (userId == Guid.Empty || p.OwnerId == userId || acceptedSharesFrom.Contains(p.OwnerId)));

        if (tiers != null && tiers.Length > 0)
            q = q.Where(p => tiers.Contains(p.Tier));

        var data = await q.Select(p => new PinDto(p.Id, p.Name, p.Tier, p.Quantity, p.Color, p.Symbol, p.X, p.Y, p.OwnerId)).ToListAsync();
        return Ok(data);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePinRequest req)
    {
        var userId = Guid.Parse(_um.GetUserId(User)!);
        var pin = new Pin
        {
            Name = req.Name.Trim(),
            Tier = req.Tier,
            Quantity = req.Quantity,
            X = Math.Clamp(req.X, 0, 1),
            Y = Math.Clamp(req.Y, 0, 1),
            OwnerId = userId
        };
        var tierColor = await _db.TierDefinitions.FindAsync(req.Tier);
        pin.Color = tierColor?.ColorHex ?? "#FF69B4";

        _db.Pins.Add(pin);
        await _db.SaveChangesAsync();
        var dto = new PinDto(pin.Id, pin.Name, pin.Tier, pin.Quantity, pin.Color, pin.Symbol, pin.X, pin.Y, pin.OwnerId);
        await _hub.Clients.All.PinCreated(dto);
        return Ok(dto);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePinRequest req)
    {
        var userId = Guid.Parse(_um.GetUserId(User)!);
        var pin = await _db.Pins.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (pin == null) return NotFound();
        if (pin.OwnerId != userId) return Forbid();

        pin.Name = req.Name.Trim();
        pin.Tier = req.Tier;
        pin.Quantity = req.Quantity;
        pin.X = Math.Clamp(req.X, 0, 1);
        pin.Y = Math.Clamp(req.Y, 0, 1);
        pin.UpdatedUtc = DateTime.UtcNow;
        var tierColor = await _db.TierDefinitions.FindAsync(req.Tier);
        pin.Color = tierColor?.ColorHex ?? "#FF69B4";

        await _db.SaveChangesAsync();
        var dto = new PinDto(pin.Id, pin.Name, pin.Tier, pin.Quantity, pin.Color, pin.Symbol, pin.X, pin.Y, pin.OwnerId);
        await _hub.Clients.All.PinUpdated(dto);
        return Ok(dto);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(_um.GetUserId(User)!);
        var pin = await _db.Pins.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (pin == null) return NotFound();
        if (pin.OwnerId != userId) return Forbid();

        pin.IsDeleted = true;
        await _db.SaveChangesAsync();
        await _hub.Clients.All.PinDeleted(id);
        return NoContent();
    }

    private Guid GetUserIdOrDefault()
    {
        var id = _um.GetUserId(User);
        return id == null ? Guid.Empty : Guid.Parse(id);
    }

}

[ApiController]
[Route("api/[controller]")]
public class SharesController : ControllerBase
{
    private readonly HuntMapContext _db;
    private readonly UserManager<ApplicationUser> _um;

    public SharesController(HuntMapContext db, UserManager<ApplicationUser> um)
    {
        _db = db; _um = um;
    }

    [Authorize]
    [HttpPost("invite")]
    public async Task<IActionResult> Invite([FromBody] ShareInviteRequest req)
    {
        var ownerId = Guid.Parse(_um.GetUserId(User)!);
        var recipient = await _um.FindByEmailAsync(req.Email);
        if (recipient == null) return BadRequest("Recipient account not found.");
        if (recipient.Id == ownerId) return BadRequest("Cannot share with yourself.");

        var existing = await _db.Shares.FirstOrDefaultAsync(s => s.OwnerId == ownerId && s.RecipientId == recipient.Id);
        if (existing != null) { existing.Status = ShareStatus.Pending; await _db.SaveChangesAsync(); return Ok(existing.Id); }

        var share = new Share { OwnerId = ownerId, RecipientId = recipient.Id, Status = ShareStatus.Pending };
        _db.Shares.Add(share);
        await _db.SaveChangesAsync();
        return Ok(share.Id);
    }

    [Authorize]
    [HttpGet("incoming")]
    public async Task<IActionResult> Incoming()
    {
        var me = Guid.Parse(_um.GetUserId(User)!);
        var list = await _db.Shares.Where(s => s.RecipientId == me).ToListAsync();
        return Ok(list);
    }

    [Authorize]
    [HttpPost("decide")]
    public async Task<IActionResult> Decide([FromBody] ShareDecisionRequest req)
    {
        var me = Guid.Parse(_um.GetUserId(User)!);
        var share = await _db.Shares.FirstOrDefaultAsync(s => s.Id == req.ShareId && s.RecipientId == me);
        if (share == null) return NotFound();
        switch (req.Decision.ToLowerInvariant())
        {
            case "accept": share.Status = ShareStatus.Accepted; break;
            case "reject": share.Status = ShareStatus.Rejected; break;
            case "block": share.Status = ShareStatus.Blocked; break;
            default: return BadRequest("Invalid decision.");
        }
        await _db.SaveChangesAsync();
        return Ok();
    }
}

public interface IPinClient
{
    Task PinCreated(PinDto dto);
    Task PinUpdated(PinDto dto);
    Task PinDeleted(Guid id);
}

public class PinHub : Microsoft.AspNetCore.SignalR.Hub<IPinClient>
{
}