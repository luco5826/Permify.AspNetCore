using Permify.AspNetCore.Model;

namespace Permify.AspNetCore.Interfaces;

public interface IPermifyAuthorizationService
{
    /// <summary>
    /// Creates a new relationship between the given <paramref name="entity"/> and <paramref name="subject"/> 
    /// </summary>
    /// <param name="entity">The <seealso cref="Entity"/> of the relationship</param>
    /// <param name="relation">The name of the relation within the <seealso cref="Entity"/></param>
    /// <param name="subject">The <seealso cref="Subject"/> of the relationship</param>
    /// <param name="tenant">The tenant in which to apply the operation (defaults to "t1")</param>
    /// <returns>A string representing the snap token returned by Permify</returns>
    Task<string> CreateRelationship(Entity entity, string relation, Subject subject, String tenant = "t1");

    /// <summary>
    /// Deletes an existing relationship between the given <paramref name="entity"/> and <paramref name="subject"/> 
    /// </summary>
    /// <param name="entity">The <seealso cref="Entity"/> of the relationship</param>
    /// <param name="relation">The name of the relation within the <seealso cref="Entity"/></param>
    /// <param name="subject">The <seealso cref="Subject"/> of the relationship</param>
    /// <param name="tenant">The tenant in which to apply the operation (defaults to "t1")</param>
    /// <returns>A string representing the snap token returned by Permify</returns>
    Task<string> DeleteRelationship(Entity entity, string relation, Subject subject, String tenant = "t1");

    /// <summary>
    /// Retrieve the collection of <see cref="Subject"/> that are connected to the given <paramref name="entity"/> through <paramref name="relation"/>
    /// </summary>
    /// <param name="entity">The <seealso cref="Entity"/> of the relationship</param>
    /// <param name="relation">The name of the relation within the <seealso cref="Entity"/></param>
    /// <param name="tenant">The tenant in which to apply the operation (defaults to "t1")</param>
    /// <returns>The collection of <see cref="Subject"/> connected to the entity</returns>
    Task<IEnumerable<Subject>> ReadRelationship(Entity entity, string relation, String tenant = "t1");

    /// <summary>
    /// Lookup the schema retrieving the collection of actions that a given relation can perform
    /// </summary>
    /// <param name="entityType">The <see cref="Entity"/> to lookup</param>
    /// <param name="relationNames">The collection of relations to lookup</param>
    /// <param name="tenant">The tenant in which to apply the operation (defaults to "t1")</param>
    /// <returns>The collection of action that the given relations can perform</returns>
    Task<IEnumerable<string>> SchemaLookup(string entityType, IEnumerable<string> relationNames, String tenant = "t1");

    /// <summary>
    /// Check whether or not the given <paramref name="subject"/> is allowed to execute the given <paramref name="action"/> on the given <paramref name="entity"/>
    /// </summary>
    /// <param name="subject">The <seealso cref="Subject"/> that wants to execute the action</param>
    /// <param name="action">The action itself</param>
    /// <param name="entity">The <seealso cref="Entity"/> on which the subject want to execute the action</param>
    /// <param name="tenant">The tenant in which to apply the operation (defaults to "t1")</param>
    /// <returns>True if the subject is allowed, false otherwise</returns>
    Task<bool> Can(Subject subject, string action, Entity entity, String tenant = "t1");
}
