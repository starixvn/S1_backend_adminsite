import React from 'react';

export default ({ Title, Options, Selected, OnChange, dataColumn, mainKey = null, disabled = false }) => {
    let opt = Options.map((o, oIdx) => {
        let key = `${Title}-${o.value}${mainKey === null ? '' : `-${mainKey}`}`;
        return <RadioBtn
            key={key}
            Id={key}
            DisplayText={o.displayText}
            Value={`${o.value}${mainKey === null ? '' : `-${mainKey}`}`}
            dataColumn={dataColumn}
            Name={`${Title}${mainKey === null ? '' : `-${mainKey}`}`}
            OnChange={OnChange.bind(this, o.value, dataColumn, mainKey, oIdx)}
            Checked={Selected === o.value}
            Disabled={disabled}
        />
    });
    return (
        <>
            <div className="row-flex">
                <div className="input-item">
                    <label className="label">{Title}</label>
                </div>
            </div>
            <div className="row-flex">{opt}</div>
        </>
    )
}


const RadioBtn = ({ Id, DisplayText, Value, Name, OnChange, Checked, Disabled }) => {
    return (
        <div className="custom-control custom-radio custom-control-inline">
            <input type="radio" id={Id} name={Name} className="custom-control-input revert-style" value={Value} onChange={OnChange} checked={Checked} disabled={Disabled} />
            <label className="custom-control-label" htmlFor={Id}>{DisplayText}</label>
        </div>
    )
}
