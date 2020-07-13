import React from 'react';

export default ({Title, Options, Selected, OnChange, dataColumn})=>{
    let opt = Options.map(o=>{
        return <RadioBtn 
            key={`${Title}-${o.value}`} 
            Id={`${Title}-${o.value}`} 
            DisplayText={o.displayText}
            Value={o.value} dataColumn={dataColumn}
            Name={Title}
            OnChange={OnChange.bind(this, o.value, dataColumn)}
            Checked={Selected === o.value} />
    });

    return(
        <div className="input-item">
            <div className="checkBoxTitle">{Title}</div>
            {opt}
        </div>
    )
}


const RadioBtn = ({Id, DisplayText, Value, Name, OnChange, Checked, Disabled})=>{
    return(
        <div className="custom-control custom-radio custom-control-inline">
            <input type="radio" id={Id} name={Name} className="custom-control-input revert-style" value={Value} onChange={OnChange} checked={Checked} disabled={Disabled}/>
            <label className="custom-control-label" htmlFor={Id}>{DisplayText}</label>
        </div>
    )
}
