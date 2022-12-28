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
    /// <returns>A string representing the snap token returned by Permify</returns>
    Task<string> CreateRelationship(Entity entity, string relation, Subject subject);

    /// <summary>
    /// Check whether or not the given <paramref name="subject"/> is allowed to execute the given <paramref name="action"/> on the given <paramref name="entity"/>
    /// </summary>
    /// <param name="subject">The <seealso cref="Subject"/> that wants to execute the action</param>
    /// <param name="action">The action itself</param>
    /// <param name="entity">The <seealso cref="Entity"/> on which the subject want to execute the action</param>
    /// <returns>True if the subject is allowed, false otherwise</returns>
    Task<bool> Can(Subject subject, string action, Entity entity);
}
