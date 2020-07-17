import React from 'react'

export default ({Title, Options, Selected=[], OnChange, dataColumn, mainKey})=>{
    let opt = Options.map(o=>{
        return <CheckBox key={`${Title}-${o.value}${mainKey === null ? '': `-${mainKey}`}`} onChange={OnChange} Title={o.displayText} checked={Selected.includes(o.value)} Value={o.value} dataColumn={dataColumn}/>
    });
    return(
        <>
            <div className="row-flex">
                <div className="input-item">
                    <label className="label">{Title}</label>
                </div>
            </div>
            <div className="row-flex">
                {opt}
            </div>
        </>
    )
}

const CheckBox = (props)=>{    
    const OnChange = (e)=>{
        e.preventDefault();
        props.onChange(props.Value, props.dataColumn);
    }
    return (
        // <span className="label" style={{'cursor':'pointer'}} onClick={OnChange}>
        //     <label className="checkbox">
        //         <input type="checkbox" value={props.Value} />
        //         <i className="icon icon-checkbox">{props.checked ? '☑' : '☐'}</i>
        //     </label>{props.Title}
        // </span>
            <label className="checkbox checkbox-checked" >
                <input type="checkbox" value={props.Value} checked={props.checked} onChange={OnChange}/>
                <i className="icon icon-checkbox"></i>
                <span className="text">{props.Title}</span>
            </label>
    )
}