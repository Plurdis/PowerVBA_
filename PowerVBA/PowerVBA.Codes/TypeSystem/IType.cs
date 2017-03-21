﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerVBA.Codes.TypeSystem
{
    /// <summary>
    /// 이 인터페이스는 타입 시스템의 처리된 타입을 나타냅니다.
    /// </summary>
    public interface IType : IEquatable<IType>
    {
        /// <summary>
        /// 타입 종류를 가져옵니다.
        /// </summary>
        TypeKind Kind { get; }

        /// <summary>
        /// 타입이 참조 형식인지 아니면 값 형식인지를 가져옵니다.
        /// </summary>
        /// <remarks>
        /// true라면 참조 형식입니다.
        /// false라면 값 형식입니다.
        /// null이라면 타입은 알 수 없습니다. (제약 되지 않은 제네릭 형식 매개 변수 (T), 또는 찾을 수 없는 형식)
        /// </remarks>
        bool? IsReferenceType { get; }

        
        //ITypeDefinition GetDefinition();


        /// <summary>
        /// 만약 이게 내포된 타입이라면 부모의 타입을 가져옵니다.
        /// </summary>
        IType DeclaringType { get; }

        /// <summary>
		/// 타입 파라미터들의 갯수를 가져옵니다.
		/// </summary>
		int TypeParameterCount { get; }

        /// <summary>
        /// 이 형식에 전달된 형식 인수를 가져옵니다.
        /// 이 유형이 매개 변수화 되지 않은 제네릭 유형 정의(T)인 경우 이 속성은 해당 유형이 자체 유형 인수로 매개 변수화 된 것 처럼 유형 매개 변수를 반환합니다.
        /// </summary>
        IList<IType> TypeArguments { get; }

        /// <summary>
        /// true라면 유형은 제네릭 형식의 인스턴스를 나타냅니다.
        /// </summary>
        bool IsParameterized { get; }

        IEnumerable<IMethod> GetMethods(Predicate<IUnresolvedMethod> filter = null);

        IEnumerable<IMethod> GetMethods(IList<IType> typeArguments, Predicate<IUnresolvedMethod> filter = null);

        IEnumerable<IProperty> GetProperties(Predicate<IUnresolvedMethod> filter = null);

        IEnumerable<IField> GetFields(Predicate<IUnresolvedMethod> filter = null);

        IEnumerable<IEvent> GetEvents(Predicate<IUnresolvedEvent> filter = null);

        IEnumerable<IMember> GetMembers(Predicate<IUnresolvedMember> filter = null);
    }
}
