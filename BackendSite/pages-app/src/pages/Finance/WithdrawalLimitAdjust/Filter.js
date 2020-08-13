import React, { useState } from 'react';
import { InputField } from './component/InputBox';
import { RadioBtn } from './component/RadioBox';
import { FilterColumns } from './defColumns';

const Panel = (props) => {
    const [stateDatas, setDatas] = useState({ [FilterColumns.userName.dataColumns]: '' });

    const EditField = (v, dataColumn) => {
        setDatas({
            ...stateDatas,
            [dataColumn]: v
        });
    }

    const ClickSubmit = () => {
        props.ClickSubmit(stateDatas);
    }
    let siteRadio = ""

    if (props.Site.length === 0) {
        return <></>
    } else {
        siteRadio = props.Site.map((value) => {
            return (
                <RadioBtn
                    key={`site-${value.siteId}`}
                    Id={`site-${value.siteId}`}
                    value={value.siteId}
                    displayText={value.siteName}
                    OnChange={EditField.bind(null, value.siteId, 'optSite')}
                    // Disabled={stateSend}
                    Checked={stateDatas.optSite === value.siteId} />
            )
        });
    }

    return (
        <div className="filter">
            <div className="form-group">
                <label className="col-form-label d-block">Site Name</label>
                {siteRadio}
            </div>
            <InputField
                IptType="text"
                columnTitle="UserName"
                defaultValue=""
                dataColumn={FilterColumns.userName.dataColumns}
                onBlur={EditField}
                OnPressEnter={ClickSubmit}
            />
            <InputField
                IptType="text"
                columnTitle="SabaId"
                defaultValue=""
                dataColumn={FilterColumns.sabaId.dataColumns}
                onBlur={EditField}
                OnPressEnter={ClickSubmit}
            />
            <div className="row-flex">
                <div className="button-group">
                    <button className="btn btn-search" type="submit" onClick={ClickSubmit}>Submit</button>
                </div>
            </div>
        </div>
    )
}

export default Panel;