using Content.Server.Shuttles.Components;
using Content.Shared.Shuttles.Components;
using Robust.Shared.Random;
using Content.Shared.Examine;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Server.Shuttles.Systems;

/// <summary>
/// System that handles shuttle designations in the format XX-### (2 letters and 3 numbers)
/// </summary>
public sealed class ShipDesignationSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    
    /// <summary>
    /// Pool of letters to use for the designation (defaults to uppercase alphabet)
    /// </summary>
    private readonly string _letterPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<ShipDesignationComponent, ComponentInit>(OnShipDesignationInit);
        SubscribeLocalEvent<ShipDesignationComponent, ComponentStartup>(OnShipDesignationStartup);
        SubscribeLocalEvent<ShipDesignationComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<ShipDesignationComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerbs);
    }
    
    private void OnShipDesignationInit(EntityUid uid, ShipDesignationComponent component, ComponentInit args)
    {
        // Generate designation if missing and auto-generate is enabled
        if (component.Designation == null && component.AutoGenerate)
        {
            GenerateDesignation(uid, component);
        }
    }
    
    private void OnShipDesignationStartup(EntityUid uid, ShipDesignationComponent component, ComponentStartup args)
    {
        UpdateEntityName(uid, component);
    }
    
    private void OnExamined(EntityUid uid, ShipDesignationComponent component, ExaminedEvent args)
    {
        if (!string.IsNullOrEmpty(component.Designation))
        {
            args.PushMarkup(Loc.GetString("ship-designation-examine", ("designation", component.Designation)));
        }
    }
    
    private void OnGetAlternativeVerbs(EntityUid uid, ShipDesignationComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("ship-designation-verb-regenerate"),
            Act = () => GenerateDesignation(uid, component)
        });
    }
    
    /// <summary>
    /// Generates a random ship designation in the format XX-### and updates the entity name
    /// </summary>
    public void GenerateDesignation(EntityUid uid, ShipDesignationComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;
        
        // Generate 2 random letters from the letter pool
        var letters = new char[2];
        for (int i = 0; i < 2; i++)
        {
            letters[i] = _letterPool[_random.Next(0, _letterPool.Length)];
        }
        
        // Generate 3 random numbers
        var numbers = _random.Next(0, 1000).ToString("D3");
        
        // Combine into XX-### format
        component.Designation = $"{letters[0]}{letters[1]}-{numbers}";
        
        UpdateEntityName(uid, component);
    }
    
    /// <summary>
    /// Updates the entity name with the designation
    /// </summary>
    private void UpdateEntityName(EntityUid uid, ShipDesignationComponent? component = null)
    {
        if (!Resolve(uid, ref component) || string.IsNullOrEmpty(component.Designation))
            return;
            
        var metadata = EntityManager.GetComponent<MetaDataComponent>(uid);
        
        // Avoid duplicating designations if the name already has one
        var baseName = metadata.EntityName;
        if (baseName.Contains(component.Designation))
            return;
            
        // Only append the designation if the name doesn't already have a different one
        foreach (var existingDesignation in baseName.Split(' '))
        {
            if (existingDesignation.Length >= 5 && existingDesignation.Contains('-') && 
                existingDesignation.IndexOf('-') == 2 && existingDesignation.Length >= 6)
            {
                // Already has a designation pattern, replace it
                baseName = baseName.Replace(existingDesignation, component.Designation);
                _metaData.SetEntityName(uid, baseName);
                return;
            }
        }
        
        // Append designation to name
        _metaData.SetEntityName(uid, $"{baseName} {component.Designation}");
    }
} 