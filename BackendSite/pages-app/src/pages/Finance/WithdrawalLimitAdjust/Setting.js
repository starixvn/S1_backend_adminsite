import React, {useState} from 'react';
import {SettingColumns, adjustTypeOpt} from './defColumns';
import {InputField} from './component/InputBox';
import {RadioBtn} from './component/RadioBox';
import {TextArea} from './component/Textarea'

const elemType = {
    radioBtn:'radioBtn',
    inputBox:'inputBox',
    textArea:'textArea'
}

const defCols = [
    {...SettingColumns.actionType, Options:adjustTypeOpt, elemType:elemType.radioBtn},
    {...SettingColumns.amount, elemType:elemType.inputBox},
    {...SettingColumns.note, elemType:elemType.textArea}
]

const Setting = (props)=>{
    const [stateDatas, updateDatas] = useState({
        [SettingColumns.actionType.dataColumns]:null,
        [SettingColumns.amount.dataColumns]:'',
        [SettingColumns.note.dataColumns]:''
    });

    const EditField = (v, dataColumns)=>{
        updateDatas({
            ...stateDatas,
            [dataColumns]:v
        })
    }

    const onSetWithdrawalLimit = ()=>{
        if(isNaN(stateDatas.amount)){
            alert('amount only allow numbers.');
        }else if(stateDatas.actionType == 2 && props.differenceAmount < parseFloat(stateDatas.amount)){
            alert('debit amount can not more then difference amount');
        }else{
            props.onSetWithdrawalLimit(stateDatas)
            updateDatas({
                [SettingColumns.actionType.dataColumns]:null,
                [SettingColumns.amount.dataColumns]:'',
                [SettingColumns.note.dataColumns]:''
            });
        }
    }

    let rows = defCols.map((col, idx)=>{
        let row = <></>;
        let key = `setting-col-${col.dataColumns}`;
        switch(col.elemType){
            case elemType.radioBtn:
                row = col.Options.map(((item, j)=>{
                    return (
                        <RadioBtn 
                            key={key+j}
                            Id={`Setting-Radio-${col.dataColumns}-${j}`}
                            value={item.value}
                            displayText={item.displayText}
                            Checked={stateDatas[col.dataColumns] == item.value}
                            OnChange={EditField.bind(null, item.value, col.dataColumns)}
                        />
                    );
                }));
                break;
            case elemType.inputBox:
                row = (
                    <InputField
                        IptType="Number"
                        key={key}
                        columnTitle={col.displayText}
                        Value={stateDatas[col.dataColumns]}
                        dataColumn={col.dataColumns}
                        onChange={EditField}
                    />
                );
                break;
            case elemType.textArea:
                row = (
                    <TextArea
                        key={key}
                        displayText={col.displayText}
                        Value={stateDatas[col.dataColumns]}
                        dataColumn={col.dataColumns}
                        onBlur={EditField}
                    />
                );
        }

        return row
    })

    return(
        <div className="form-group">
            <label className="col-form-label d-block">Option</label>
            {rows}
            <div className="button-group mt-10">
                <button className="btn btn-primary" onClick={onSetWithdrawalLimit}>Submit</button>
            </div>
        </div>
    )
}

export default Setting;