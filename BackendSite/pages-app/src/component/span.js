import React from 'react'

export const SpanClassName = {
    default:'',
    textPrimary:'text-primary',
    textHighlight:'text-highlight',
    textBold:'text-bold'
}

export const Span = ({SpanClass=[], ...props})=>{
    let _className = SpanClass ? ' '+SpanClass.join(' ') : null;
    return <span className={`text${_className}`}>{props.DisplayText}</span>
}