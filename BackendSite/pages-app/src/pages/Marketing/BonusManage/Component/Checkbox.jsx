import React from 'react'

export default ({Title, Options, Selected=[], OnChange, dataColumn})=>{
    let opt = Options.map(o=>{
        return <CheckBox key={`${Title}-${o.value}`} onChange={OnChange} Title={o.displayText} checked={Selected.includes(o.value)} Value={o.value} dataColumn={dataColumn}/>
    });

    return(
        <div className="input-item">
            <div className="checkBoxTitle">{Title}</div>
            {opt}
        </div>
    )
}

const CheckBox = (props)=>{
    const OnChange = (e)=>{
        e.preventDefault();
        props.onChange(props.Value, props.dataColumn);
    }
    return (
        <span className="label" style={{'cursor':'pointer'}} onClick={OnChange}>
            <label className="checkbox">
                <input type="checkbox" value={props.Value} />
                <i className="icon icon-checkbox">{props.checked ? '☑' : '☐'}</i>
            </label>{props.Title}
        </span>
    )
}