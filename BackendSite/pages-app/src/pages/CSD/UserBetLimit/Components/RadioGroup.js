import React from 'react';
import RadioButton from './RadioButton'

export default function SiteRadioGroup ({ title, siteList, selectSite, disabled, toggle = f => f }) {
    let site = siteList.map((o, oIdx)=>{
        return <RadioButton 
                    key={oIdx}
                    id={oIdx}
                    title={o.siteName}
                    value={o.siteId}
                    checked={selectSite === o.siteId}
                    disabled={disabled}
                    toggle={toggle} />
        });
    return(
        <>
            <div className="row-flex">
                <div className="input-item">
                    <label className="label">{title}</label>
                </div>
            </div>
            <div className="row-flex">{site}</div>
        </>
    )
}
