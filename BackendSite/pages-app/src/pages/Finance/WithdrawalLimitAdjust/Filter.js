import React, {useState} from 'react';
import {InputField} from './component/InputBox';
import {FilterColumns} from './defColumns';

const Panel = (props)=>{
    const [stateDatas, setDatas] = useState({[FilterColumns.userName.dataColumns]:''});

    const EditField = (v, dataColumn)=>{
        setDatas({
            ...stateDatas,
            [dataColumn]: v
        });
    }

    const ClickSubmit = ()=>{
        props.ClickSubmit(stateDatas);
    }

    return(
        <div className="filter">
            <div className="row-flex">
                <InputField
                    IptType="text"
                    columnTitle="UserName"
                    defaultValue=""
                    dataColumn={FilterColumns.userName.dataColumns}
                    onBlur={EditField}
                    OnPressEnter={ClickSubmit}
                />
                <div className="button-group">
                    <button className="btn btn-search" type="submit" onClick={ClickSubmit}>Submit</button>
                </div>
            </div>
        </div>
    )
}

export default Panel;