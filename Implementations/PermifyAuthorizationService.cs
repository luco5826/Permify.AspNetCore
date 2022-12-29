using Grpc.Net.Client;
using Permify.AspNetCore.Extensions;
using Permify.AspNetCore.Interfaces;
using Permify.AspNetCore.Model;

namespace Permify.AspNetCore.Implementations;

public class PermifyAuthorizationService : IPermifyAuthorizationService
{
    private Base.V1.Permission.PermissionClient _permissionClient;
    private Base.V1.Relationship.RelationshipClient _relationshipClient;

    public PermifyAuthorizationService(PermifyOptions options)
    {
        var channel = GrpcChannel.ForAddress(options?.Host ?? "");

        _permissionClient = new Base.V1.Permission.PermissionClient(channel);
        _relationshipClient = new Base.V1.Relationship.RelationshipClient(channel);
    }

    public async Task<string> CreateRelationship(Entity entity, string relation, Subject subject)
    {
        var tupleList = new Google.Protobuf.Collections.RepeatedField<Base.V1.Tuple>();
        tupleList.Add(
            new Base.V1.Tuple
            {
                Entity = new Base.V1.Entity { Id = entity.Id, Type = entity.Type },
                Relation = relation,
                Subject = new Base.V1.Subject { Id = subject.Id, Type = subject.Type, Relation = subject.Relation },
            });

        var req = new Base.V1.RelationshipWriteRequest
        {
            Metadata = new Base.V1.RelationshipWriteRequestMetadata
            {
                SchemaVersion = "",
            },
        };
        req.Tuples.AddRange(tupleList);

        var response = await _relationshipClient.WriteAsync(req);
        return response.SnapToken;
    }

    public async Task<string> DeleteRelationship(Entity entity, string relation, Subject subject)
    {
        // Build the EntityFilter
        var entityFilter = new Base.V1.EntityFilter { Type = entity.Type };
        entityFilter.Ids.Add(entity.Id);

        // Build the SubjectFilter
        var subjectFilter = new Base.V1.SubjectFilter { Type = subject.Type, Relation = subject.Relation };
        subjectFilter.Ids.Add(subject.Id);

        // Perform the action
        var response = await _relationshipClient.DeleteAsync(new Base.V1.RelationshipDeleteRequest
        {
            Filter = new Base.V1.TupleFilter
            {
                Entity = entityFilter,
                Relation = relation,
                Subject = subjectFilter
            },
        });

        return response.SnapToken;
    }

    public async Task<bool> Can(Subject subject, string action, Entity entity)
    {
        var response = await _permissionClient.CheckAsync(
            new Base.V1.PermissionCheckRequest
            {
                Metadata = new Base.V1.PermissionCheckRequestMetadata
                {
                    SchemaVersion = "",
                    Depth = 3,
                },
                Subject = new Base.V1.Subject
                {
                    Id = subject.Id,
                    Type = subject.Type,
                },
                Permission = action,
                Entity = new Base.V1.Entity
                {
                    Id = entity.Id,
                    Type = entity.Type,
                }
            }
        );

        return response.Can == Base.V1.PermissionCheckResponse.Types.Result.Allowed;
    }
}