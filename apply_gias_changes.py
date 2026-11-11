#!/usr/bin/env python3
"""Apply targeted GIAS group-membership updates to a checked-out development branch.
Aborts rather than guessing if upstream code has changed.
"""
from pathlib import Path
import sys

root = Path(sys.argv[1] if len(sys.argv)>1 else '.').resolve()
changes={}
def edit(name, before, after, expected_count=1):
    p=root/name
    original=changes.get(p,p.read_text())
    if before not in original:
        raise RuntimeError(f'Missing expected source in {name}: {before[:100]!r}')
    if original.count(before)!=expected_count:
        raise RuntimeError(f'Ambiguous source in {name}: {before[:100]!r}')
    changes[p]=original.replace(before, after)


import re
def edit_method(name, method_name, replacement):
    p = root / name
    original = changes.get(p, p.read_text())
    pattern = rf"\bpublic\s+async\s+Task<GroupEstablishmentModel>\s+{re.escape(method_name)}\s*\("
    matches = list(re.finditer(pattern, original))
    if len(matches) != 1:
        raise RuntimeError(f"Expected one {method_name} in {name}, found {len(matches)}")
    start = matches[0].start()
    opening = original.find('{', matches[0].end())
    if opening < 0:
        raise RuntimeError(f"Missing method body: {name}")
    depth = 0
    ending = None
    for pos in range(opening, len(original)):
        if original[pos] == '{': depth += 1
        elif original[pos] == '}':
            depth -= 1
            if depth == 0:
                ending = pos + 1
                break
    if ending is None:
        raise RuntimeError(f"Unterminated method body: {name}")
    changes[p] = original[:start] + replacement + original[ending:]

sql='src/Dfe.PlanTech.Data.Sql/'
app='src/Dfe.PlanTech.Application/'
web='src/Dfe.PlanTech.Web/'
edit(sql+'Interfaces/IGiasRepository.cs',
'    Task<GiasEstablishmentEntity?> GetSingleAcademySchool(int groupUid);',
'''    Task<GiasEstablishmentEntity?> GetSingleAcademySchool(int groupUid);
    Task<List<GiasEstablishmentEntity>> GetSchoolsForGroupAsync(int groupUid);
    Task<bool> IsSchoolWithinGroupAsync(int groupUid, int schoolUrn);''')
edit(sql+'Repositories/GiasRepository.cs',
'''    public Task<GiasEstablishmentEntity?> GetSingleAcademySchool(int groupUid)''',
'''    public Task<List<GiasEstablishmentEntity>> GetSchoolsForGroupAsync(int groupUid)
    {
        return _db.GiasEstablishmentGroups
            .AsNoTracking()
            .Where(g => g.GroupUid == groupUid && g.GroupStatusCode == "OPEN")
            .SelectMany(g => g.GroupMemberships)
            .SelectMany(m => m.Establishments)
            .Distinct()
            .ToListAsync();
    }

    public Task<bool> IsSchoolWithinGroupAsync(int groupUid, int schoolUrn)
    {
        return _db.GiasEstablishmentGroups
            .AsNoTracking()
            .Where(g => g.GroupUid == groupUid && g.GroupStatusCode == "OPEN")
            .SelectMany(g => g.GroupMemberships)
            .SelectMany(m => m.Establishments)
            .AnyAsync(e => e.Urn == schoolUrn);
    }

    public Task<GiasEstablishmentEntity?> GetSingleAcademySchool(int groupUid)''')

edit(app+'Workflows/GroupWorkflow.cs',
'''public class GroupWorkflow(ISubmissionRepository submissionRepository, IEstablishmentService establishmentService, IEstablishmentRecommendationHistoryRepository recommendationHistoryRepository, IEstablishmentGroupRepository establishmentGroupRepository) : IGroupWorkflow''',
'''public class GroupWorkflow(ISubmissionRepository submissionRepository, IEstablishmentService establishmentService, IEstablishmentRecommendationHistoryRepository recommendationHistoryRepository, IGiasRepository giasRepository, IRecommendationWorkflow recommendationWorkflow) : IGroupWorkflow''')
edit(app+'Workflows/GroupWorkflow.cs',
'''    private readonly ISubmissionRepository _submissionRepository =''',
'''    private readonly IGiasRepository _giasRepository = giasRepository ?? throw new ArgumentNullException(nameof(giasRepository));
    private readonly IRecommendationWorkflow _recommendationWorkflow = recommendationWorkflow ?? throw new ArgumentNullException(nameof(recommendationWorkflow));
    private readonly ISubmissionRepository _submissionRepository =''')
# remove obsolete repository dependency from the workflow, including constructor arg
edit(app+'Workflows/GroupWorkflow.cs',
'''IRecommendationWorkflow recommendationWorkflow) : IGroupWorkflow''',
'''IRecommendationWorkflow recommendationWorkflow) : IGroupWorkflow''') if False else None
edit(app+'Workflows/GroupWorkflow.cs',
'''    private readonly IEstablishmentGroupRepository _establishmentGroupRepository =
        establishmentGroupRepository ?? throw new ArgumentNullException(nameof(establishmentGroupRepository));
''','')
edit_method(app+'Workflows/GroupWorkflow.cs',
    'GetGroupEstablishmentContextAsync',
    '    private async Task<int> GetGroupUidOrThrowAsync(int groupEstablishmentId)\n    {\n        var group = await _establishmentService.GetEstablishmentByIdAsync(groupEstablishmentId);\n        if (!int.TryParse(group.GroupUid, out var groupUid) || groupUid <= 0)\n            throw new InvalidDataException($"Group {groupEstablishmentId} has no valid GroupUid");\n        return groupUid;\n    }\n\n    public async Task<bool> IsSchoolWithinGroupAsync(int groupEstablishmentId, string schoolUrn)\n    {\n        if (!int.TryParse(schoolUrn, out var urn) || urn <= 0)\n            return false;\n        var groupUid = await GetGroupUidOrThrowAsync(groupEstablishmentId);\n        return await _giasRepository.IsSchoolWithinGroupAsync(groupUid, urn);\n    }\n\n    public async Task<List<SqlEstablishmentLinkDto>> GetGroupSchoolsAsync(int groupEstablishmentId, bool includeRecommendationCounts = false)\n    {\n        var context = await GetGroupEstablishmentContextAsync(groupEstablishmentId);\n        var schools = new List<SqlEstablishmentLinkDto>();\n        foreach (var school in context.Establishments)\n        {\n            var dto = new SqlEstablishmentLinkDto\n            {\n                Id = school.Id,\n                Urn = school.EstablishmentRef,\n                EstablishmentName = school.OrgName ?? string.Empty,\n                GroupUid = school.GroupUid ?? string.Empty\n            };\n            if (includeRecommendationCounts)\n            {\n                var statuses = await _recommendationWorkflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(school.Id);\n                dto.InProgressOrCompletedRecommendationsCount = statuses.Values.Count(r =>\n                    r.NewStatus == RecommendationStatus.Complete ||\n                    r.NewStatus == RecommendationStatus.InProgress);\n            }\n            schools.Add(dto);\n        }\n        return schools;\n    }\n\n    public async Task<GroupEstablishmentModel> GetGroupEstablishmentContextAsync(int groupEstablishmentId)\n    {\n        var groupUid = await GetGroupUidOrThrowAsync(groupEstablishmentId);\n        var giasSchools = await _giasRepository.GetSchoolsForGroupAsync(groupUid);\n        var establishments = new List<SqlEstablishmentDto>();\n        foreach (var school in giasSchools.DistinctBy(e => e.Urn))\n        {\n            establishments.Add(await _establishmentService.GetOrCreateEstablishmentAsync(\n                school.Urn.ToString(), school.EstablishmentName));\n        }\n        var ids = establishments.Select(e => e.Id).Distinct().ToArray();\n        var completed = ids.Length == 0 ? [] : await GetGroupCompletedSubmissions(ids);\n        return new GroupEstablishmentModel\n        {\n            Establishments = establishments,\n            EstablishmentIds = ids,\n            CompletedSubmissions = completed\n        };\n    }')
edit(app+'Workflows/GroupWorkflow.cs',
'using Dfe.PlanTech.Core.Enums;',
'using Dfe.PlanTech.Core.Enums;') if False else None

edit(app+'Workflows/Interfaces/IGroupWorkflow.cs',
'    Task<GroupEstablishmentModel> GetGroupEstablishmentContextAsync(int groupEstablishmentId);',
'''    Task<GroupEstablishmentModel> GetGroupEstablishmentContextAsync(int groupEstablishmentId);
    Task<List<SqlEstablishmentLinkDto>> GetGroupSchoolsAsync(int groupEstablishmentId, bool includeRecommendationCounts = false);
    Task<bool> IsSchoolWithinGroupAsync(int groupEstablishmentId, string schoolUrn);''')
edit(app+'Services/Interfaces/IGroupService.cs',
'    Task<GroupEstablishmentModel> GetGroupEstablishmentContextAsync(int groupEstablishmentId);',
'''    Task<GroupEstablishmentModel> GetGroupEstablishmentContextAsync(int groupEstablishmentId);
    Task<List<SqlEstablishmentLinkDto>> GetGroupSchoolsAsync(int groupEstablishmentId, bool includeRecommendationCounts = false);
    Task<bool> IsSchoolWithinGroupAsync(int groupEstablishmentId, string schoolUrn);''')
edit(app+'Services/GroupService.cs',
'''    public Task<GroupEstablishmentModel> GetGroupEstablishmentContextAsync(''',
'''    public Task<List<SqlEstablishmentLinkDto>> GetGroupSchoolsAsync(int groupEstablishmentId, bool includeRecommendationCounts = false)
        => _groupWorkflow.GetGroupSchoolsAsync(groupEstablishmentId, includeRecommendationCounts);

    public Task<bool> IsSchoolWithinGroupAsync(int groupEstablishmentId, string schoolUrn)
        => _groupWorkflow.IsSchoolWithinGroupAsync(groupEstablishmentId, schoolUrn);

    public Task<GroupEstablishmentModel> GetGroupEstablishmentContextAsync(''')

edit(app+'Providers/CurrentUserProvider.cs',
'''    private readonly IEstablishmentService _establishmentService;
''',
'''    private readonly IEstablishmentService _establishmentService;
    private readonly IGroupService _groupService;
''')
edit(app+'Providers/CurrentUserProvider.cs',
'''        IEstablishmentService establishmentService,
        ILogger<CurrentUserProvider> logger''',
'''        IEstablishmentService establishmentService,
        IGroupService groupService,
        ILogger<CurrentUserProvider> logger''')
edit(app+'Providers/CurrentUserProvider.cs',
'''        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _selectedSchoolLazy''',
'''        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _selectedSchoolLazy''')
edit(app+'Providers/CurrentUserProvider.cs',
'''            // Get all schools in the user's group
            var groupSchools =
                await _establishmentService.GetEstablishmentLinksWithRecommendationCounts(
                    UserOrganisationId.Value
                );

            var selectedSchoolIsValid = groupSchools.Any(s =>
                s.Urn.Equals(urn, StringComparison.OrdinalIgnoreCase)
            );''',
'''            var selectedSchoolIsValid = await _groupService.IsSchoolWithinGroupAsync(
                UserOrganisationId.Value, urn);''')

# Replace all reader/writer legacy lookup references in GroupsViewBuilder with a canonical GIAS-backed group list.
p=web+'ViewBuilders/GroupsViewBuilder.cs'
edit(p,
'''        var groupSchools =
            await _establishmentService.GetEstablishmentLinksWithRecommendationCounts(
                establishmentId
            );''',
'''        var groupSchools = await _groupService.GetGroupSchoolsAsync(
            establishmentId, includeRecommendationCounts: true);''')
edit(p,
'''        var establishmentLinks = await _establishmentService.GetEstablishmentLinks(
            userEstablishmentId
        );''',
'''        var establishmentLinks = await _groupService.GetGroupSchoolsAsync(userEstablishmentId);''', expected_count=2)
edit(p,
'''        var matEstablishmentLinks =
            await _establishmentService.GetEstablishmentLinks(establishmentId) ?? [];''',
'''        var matEstablishmentLinks = await _groupService.GetGroupSchoolsAsync(establishmentId);''')
edit(p,
'''        var establishmentLinks = await _establishmentService.GetEstablishmentLinks(establishmentId);''',
'''        var establishmentLinks = await _groupService.GetGroupSchoolsAsync(establishmentId);''')
edit(p,
'''        var matEstablishmentLinks =
            await _establishmentService.GetEstablishmentLinks(matEstablishmentId) ?? [];''',
'''        var matEstablishmentLinks = await _groupService.GetGroupSchoolsAsync(matEstablishmentId);''')
# Prevent partial acceptance of tampered bulk submitted schools before mutations.
# Validate the entire submitted school list before performing any mutations.
# Match the foreach independently of how the selectedSchoolIds list is declared.
def insert_selection_guard(name):
    file_path = root / name
    source = changes.get(file_path, file_path.read_text())
    method_start = source.find('RouteToSelectStatusToUpdateViewModelAsync(')
    if method_start < 0:
        raise RuntimeError(f'Cannot find status-update method in {name}')
    loop = re.search(r"(?m)^(?P<indent>[ \t]*)foreach\s*\(\s*var\s+schoolRef\s+in\s+viewModel\.SelectedSchoolsRefs\s*\)", source[method_start:])
    if loop is None:
        raise RuntimeError(f'Cannot find school selection loop in {name}; review validation manually')
    insert_at = method_start + loop.start()
    indent = loop.group('indent')
    guard = (indent + 'if (viewModel.SelectedSchoolsRefs.Any(s => !VerifyGroupSchoolMembership(s, establishmentLinks)))\n'
             + indent + '    throw new InvalidDataException("One or more selected schools are not within the user\'s group");\n\n')
    changes[file_path] = source[:insert_at] + guard + source[insert_at:]

insert_selection_guard(p)
edit(p,
'''        if (selectedRefs.Length == 1)
        {''',
'''        if (selectedRefs.Length == 0 || selectedRefs.Any(s => !VerifyGroupSchoolMembership(s, establishmentLinks)))
            throw new InvalidDataException("One or more selected schools are not within the user's group");
        if (selectedRefs.Length == 1)
        {''')
# Write only after all expected snippets match; safe to rerun only from a clean checkout.
for path, content in changes.items():
    path.write_text(content)
    print(path.relative_to(root))
print(f'Updated {len(changes)} files. Run dotnet build and tests; not executed by this script.')
