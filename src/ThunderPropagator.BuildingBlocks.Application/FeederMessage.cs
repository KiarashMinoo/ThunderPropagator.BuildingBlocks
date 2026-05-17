using Ardalis.GuardClauses;
using JetBrains.Annotations;
using ThunderPropagator.BuildingBlocks.Application.Attributes;
using ThunderPropagator.BuildingBlocks.Application.CorrelationId;
using ThunderPropagator.BuildingBlocks.Application.Enums;
using ThunderPropagator.BuildingBlocks.Application.Objects;
using System.Collections;
using System.Runtime.CompilerServices;

namespace ThunderPropagator.BuildingBlocks.Application
{
    [JsonSerialization(CamelCase = false)]
    public abstract class FeederMessage : DisposableObject,
        IDictionary<string, object?>,
        IReadOnlyDictionary<string, object?>,
        ICorrelationIdSupport,
        ICloneable,
        ICloneable<IDictionary<string, object?>>
    {
        private readonly FeederMessageEnvelope _envelope = new();
        [IgnoreMember] private readonly FeederMessagePayload _payload = new();

        /// <summary>
        /// Gets the protocol-level header of this message. Infrastructure code may accept
        /// <see cref="FeederMessageEnvelope"/> directly to reduce coupling to concrete message types.
        /// </summary>
        [IgnoreMember]
        public FeederMessageEnvelope Envelope => _envelope;

        /// <summary>Gets the user-defined field store of this message.</summary>
        [IgnoreMember]
        public FeederMessagePayload Payload => _payload;

        [IgnoreMember]
        public object? this[string key]
        {
            get => _payload.GetValueOrNull<object>(key)!;
            set => _payload.SetValue(value, Guard.Against.NullOrWhiteSpace(key));
        }

        bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => false;

        int ICollection<KeyValuePair<string, object?>>.Count => ((IDictionary<string, object?>)_payload).Count;
        int IReadOnlyCollection<KeyValuePair<string, object?>>.Count => ((IReadOnlyDictionary<string, object?>)_payload).Count;

        ICollection<string> IDictionary<string, object?>.Keys => ((IDictionary<string, object?>)_payload).Keys;
        IEnumerable<string> IReadOnlyDictionary<string, object?>.Keys => ((IReadOnlyDictionary<string, object?>)_payload).Keys;
        ICollection<object?> IDictionary<string, object?>.Values => ((IDictionary<string, object?>)_payload).Values;
        IEnumerable<object?> IReadOnlyDictionary<string, object?>.Values => ((IReadOnlyDictionary<string, object?>)_payload).Values;

        internal int? HashKey
        {
            get => _envelope.HashKey;
            set => _envelope.HashKey = value;
        }

        public CastType CastType
        {
            get => _envelope.CastType;
            set => _envelope.CastType = value;
        }

        public bool IsDeleted
        {
            get => _envelope.IsDeleted;
            set => _envelope.IsDeleted = value;
        }

        public string CorrelationId
        {
            get => _envelope.CorrelationId;
            set => _envelope.CorrelationId = value;
        }

        protected FeederMessage()
        {
            CastType = CastType.Multicast;
        }

        protected void SetValue(object? value, [CallerMemberName] string? key = null)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            _payload.SetValue(value, Guard.Against.NullOrWhiteSpace(key));
        }

        protected T GetValue<T>([CallerMemberName] string? key = null)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return _payload.GetValue<T>(Guard.Against.NullOrWhiteSpace(key));
        }

        protected T? GetValueOrNull<T>([CallerMemberName] string? key = null)
            => _payload.GetValueOrNull<T>(Guard.Against.NullOrWhiteSpace(key));

        protected T GetValueOrDefault<T>(T @default, [CallerMemberName] string? key = null)
            => _payload.GetValueOrDefault(@default, Guard.Against.NullOrWhiteSpace(key));

        object ICloneable.Clone() => MemberwiseClone();
        IDictionary<string, object?> ICloneable<IDictionary<string, object?>>.Clone() => _payload.ToDictionary();

        void IDictionary<string, object?>.Add(string key, object? value) => ((IDictionary<string, object?>)_payload).Add(key, value);
        void ICollection<KeyValuePair<string, object?>>.Add(KeyValuePair<string, object?> item) => ((ICollection<KeyValuePair<string, object?>>)_payload).Add(item);
        void ICollection<KeyValuePair<string, object?>>.Clear() => throw new NotSupportedException();
        bool IDictionary<string, object?>.Remove(string key) => throw new NotSupportedException();
        bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item) => throw new NotSupportedException();
        bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item) => ((ICollection<KeyValuePair<string, object?>>)_payload).Contains(item);
        bool IDictionary<string, object?>.ContainsKey(string key) => ((IDictionary<string, object?>)_payload).ContainsKey(key);
        bool IReadOnlyDictionary<string, object?>.ContainsKey(string key) => ((IReadOnlyDictionary<string, object?>)_payload).ContainsKey(key);
        bool IDictionary<string, object?>.TryGetValue(string key, out object? value) => ((IDictionary<string, object?>)_payload).TryGetValue(key, out value);
        bool IReadOnlyDictionary<string, object?>.TryGetValue(string key, out object? value) => ((IReadOnlyDictionary<string, object?>)_payload).TryGetValue(key, out value);

        void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<string, object?>>)_payload).CopyTo(array, arrayIndex);

        [MustDisposeResource]
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _payload.GetEnumerator();

        [MustDisposeResource]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override int GetHashCode() => ((IDictionary<string, object?>)_payload).Keys.Aggregate(0, HashCode.Combine);

        /// <summary>
        /// Resets all fields to their initial state so the instance can be returned to an object pool.
        /// </summary>
        public virtual void Reset()
        {
            _payload.Clear();
            _envelope.CorrelationId = string.Empty;
            _envelope.HashKey = null;
            _envelope.CastType = CastType.Multicast;
            _envelope.IsDeleted = false;
        }

        protected override void DisposeManagedResources() => _payload.Clear();
    }
}
