#region License
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceHolderSynchronization.cs" company="The original author or authors.">
//   Copyright 2002-2013 the original author or authors.
//   
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
//   the License. You may obtain a copy of the License at
//   
//   http://www.apache.org/licenses/LICENSE-2.0
//   
//   Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
//   an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
//   specific language governing permissions and limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion

namespace Spring.Transaction.Support
{
    /// <summary>
    /// <see cref="ITransactionSynchronization"/> implementation that manages a
    /// <see cref="IResourceHolder"/> bound through <see cref="TransactionSynchronizationManager"/>.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Thomas Trageser</author>
    public abstract class ResourceHolderSynchronization<TH, TK> : ITransactionSynchronization
        where TH : IResourceHolder
    {
        private readonly TH _resourceHolder;

        private readonly TK _resourceKey;

        private volatile bool _holderActive = true;

        /// <summary>
        /// Create a new ResourceHolderSynchronization for the given holder.
        /// <see cref="TransactionSynchronizationManager.BindResource"/>
        /// </summary>
        /// <param name="resourceHolder">the ResourceHolder to manage</param>
        /// <param name="resourceKey">the key to bind the ResourceHolder for</param>
        public ResourceHolderSynchronization(TH resourceHolder, TK resourceKey)
        {
            _resourceHolder = resourceHolder;
            _resourceKey = resourceKey;
        }


        /// <summary>
        /// Suspend this synchronization. 
        /// </summary>
        /// <remarks>
        /// <p>
        /// Supposed to unbind resources from
        /// <see cref="Spring.Transaction.Support.TransactionSynchronizationManager"/>
        /// if managing any.
        /// </p>
        /// </remarks>
        public void Suspend()
        {
            if (_holderActive)
            {
                TransactionSynchronizationManager.UnbindResource(_resourceKey);
            }
        }

        /// <summary>
        /// Resume this synchronization.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Supposed to rebind resources from
        /// <see cref="Spring.Transaction.Support.TransactionSynchronizationManager"/>
        /// if managing any.
        /// </p>
        /// </remarks>
        public void Resume()
        {
            if (_holderActive)
            {
                TransactionSynchronizationManager.BindResource(_resourceKey, _resourceHolder);
            }
        }

        public void Flush()
        {
            FlushResource(_resourceHolder);
        }

        /// <summary>
        /// Invoked before transaction commit (before
        /// <see cref="Spring.Transaction.Support.ITransactionSynchronization.BeforeCompletion"/>)
        /// Can e.g. flush transactional O/R Mapping sessions to the database
        /// </summary>
        /// <remarks>
        /// <para>
        /// This callback does not mean that the transaction will actually be
        /// commited.  A rollback decision can still occur after this method
        /// has been called.  This callback is rather meant to perform work 
        /// that's only relevant if a commit still has a chance
        /// to happen, such as flushing SQL statements to the database.
        /// </para>
        /// <para>
        /// Note that exceptions will get propagated to the commit caller and cause a
        /// rollback of the transaction.</para>
        /// <para>
        /// (note: do not throw TransactionException subclasses here!)
        /// </para>
        /// </remarks>
        /// <param name="readOnly">
        /// If the transaction is defined as a read-only transaction.
        /// </param>
        public void BeforeCommit(bool readOnly)
        {
        }

        /// <summary>
        /// Invoked before transaction commit/rollback (after
        /// <see cref="Spring.Transaction.Support.ITransactionSynchronization.BeforeCommit"/>,
        /// even if
        /// <see cref="Spring.Transaction.Support.ITransactionSynchronization.BeforeCommit"/>
        /// threw an exception).
        /// </summary>
        /// <remarks>
        /// <p>
        /// Can e.g. perform resource cleanup.
        /// </p>
        /// <p>
        /// Note that exceptions will get propagated to the commit caller
        /// and cause a rollback of the transaction.
        /// </p>
        /// </remarks>
        public void BeforeCompletion()
        {
            if (ShouldUnbindAtCompletion())
            {
                TransactionSynchronizationManager.UnbindResource(_resourceKey);
                _holderActive = false;
                if (ShouldReleaseBeforeCompletion())
                {
                    ReleaseResource(_resourceHolder, _resourceKey);
                }
            }
        }

        /// <summary>
        /// Invoked after transaction commit.
        /// </summary>
        /// <remarks>Can e.g. commit further operations that are supposed to follow on
        /// a successful commit of the main transaction.
        /// Throws exception in case of errors; will be propagated to the caller.
        /// Note: To not throw TransactionExeption sbuclasses here!
        /// </remarks>
        /// 
        public void AfterCommit()
        {
            if (!ShouldReleaseBeforeCompletion())
            {
                ProcessResourceAfterCommit(_resourceHolder);
            }
        }

        /// <summary>
        /// Invoked after transaction commit/rollback.
        /// </summary>
        /// <param name="status">
        /// Status according to <see cref="Spring.Transaction.Support.TransactionSynchronizationStatus"/>
        /// </param>
        /// <remarks>
        /// Can e.g. perform resource cleanup, in this case after transaction completion.
        /// <p>
        /// Note that exceptions will get propagated to the commit or rollback
        /// caller, although they will not influence the outcome of the transaction.
        /// </p>
        /// </remarks>
        public void AfterCompletion(TransactionSynchronizationStatus status)
        {
            if (ShouldUnbindAtCompletion())
            {
                bool releaseNecessary;
                if (_holderActive)
                {
                    // The thread-bound resource holder might not be available anymore,
                    // since afterCompletion might get called from a different thread.
                    _holderActive = false;
                    TransactionSynchronizationManager.UnbindResource(_resourceKey);
                    _resourceHolder.Unbound();
                    releaseNecessary = true;
                }
                else
                {
                    releaseNecessary = ShouldReleaseAfterCompletion(_resourceHolder);
                }
                if (releaseNecessary)
                {
                    ReleaseResource(_resourceHolder, _resourceKey);
                }
            }
            else
            {
                // Probably a pre-bound resource...
                CleanupResource(_resourceHolder, _resourceKey, (status == TransactionSynchronizationStatus.Committed));
            }
            _resourceHolder.Reset();
        }

        /// <summary>
        /// Return whether this holder should be unbound at completion
        /// (or should rather be left bound to the thread after the transaction).
        /// <p>The default implementation returns <code>true</code>.</p>
        /// </summary>
        protected bool ShouldUnbindAtCompletion()
        {
            return true;
        }

        /// <summary>
        /// Return whether this holder's resource should be released before
        /// transaction completion (<code>true</code>) or rather after
        /// transaction completion (<code>false</code>).
        /// <p>Note that resources will only be released when they are
        /// unbound from the thread (<see cref="ShouldUnbindAtCompletion"/>).</p>
        /// <p>The default implementation returns <code>true</code>.</p>
        /// <see cref="ReleaseResource"/>
        /// </summary>
        protected bool ShouldReleaseBeforeCompletion()
        {
            return true;
        }

        /// <summary>
        /// Return whether this holder's resource should be released after
        /// transaction completion (<code>true</code>).
        /// <p>The default implementation returns <code>!shouldReleaseBeforeCompletion()</code>,
        /// releasing after completion if no attempt was made before completion.</p>
        /// <see cref="ReleaseResource"/>
        /// </summary>
        /// <param name="resourceHolder">the resource holder to release</param>
        protected bool ShouldReleaseAfterCompletion(TH resourceHolder)
        {
            return !ShouldReleaseBeforeCompletion();
        }

        /// <summary>
        /// Flush callback for the given resource holder.
        /// </summary>
        /// <param name="resourceHolder">the resource holder to flush</param>
        protected void FlushResource(TH resourceHolder)
        {
        }

        /// <summary>
        /// After-commit callback for the given resource holder.
        /// Only called when the resource hasn't been released yet
        /// (<see cref="ShouldReleaseBeforeCompletion"/>).
        /// </summary>
        /// <param name="resourceHolder">the resource holder to process</param>
        protected void ProcessResourceAfterCommit(TH resourceHolder)
        {
        }

        /// <summary>
        /// Release the given resource (after it has been unbound from the thread).
        /// </summary>
        /// <param name="resourceHolder">the resource holder to process</param>
        /// <param name="resourceKey">the key that the ResourceHolder was bound for</param>
        protected void ReleaseResource(TH resourceHolder, TK resourceKey)
        {
        }

        /// <summary>
        /// Perform a cleanup on the given resource (which is left bound to the thread).
        /// </summary>
        /// <param name="resourceHolder">the resource holder to process</param>
        /// <param name="resourceKey">the key that the ResourceHolder was bound for</param>
        /// <param name="committed">whether the transaction has committed (<code>true</code>) or rolled back  (<code>false</code>)</param>
        protected void CleanupResource(TH resourceHolder, TK resourceKey, bool committed)
        {
        }

    }
}
