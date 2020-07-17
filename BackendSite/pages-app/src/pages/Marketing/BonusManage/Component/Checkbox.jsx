import React from 'react'

export default ({Title, Options, Selected=[], OnChange, dataColumn})=>{
    let opt = Options.map(o=>{
        return <CheckBox key={`${Title}-${o.value}`} onChange={OnChange} Title={o.displayText} checked={Selected.includes(o.value)} Value={o.value} dataColumn={dataColumn}/>
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
        <div className="custom-radio custom-control-inline" >
            <label className="checkbox checkbox-checked" >
                <input type="checkbox" value={props.Value} checked={props.checked} onChange={OnChange}/>
                <span className="text">{props.Title}</span>
            </label>
        </div>
    )
}