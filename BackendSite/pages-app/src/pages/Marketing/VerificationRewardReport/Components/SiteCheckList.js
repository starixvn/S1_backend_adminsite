import React from "react"
import Checkbox from "./Checkbox"

export default function SiteCheckList({ siteList, siteChecked = f => f }) {
    return (
        <div className="input-item" style={{ width: "auto" }}>
            <label className="label">Site</label>
            {siteList.map((site, i) => { 
                return (
                    <Checkbox 
                        key={i}
                        title={site.siteName}
                        value={site.siteId}
                        checked={site.checked}
                        toggle = {siteChecked}/>
                )}
            )}
        </div>
    )
}