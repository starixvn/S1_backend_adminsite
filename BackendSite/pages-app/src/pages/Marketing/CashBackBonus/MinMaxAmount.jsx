import React, {memo} from 'react';
import InputBox from './Component/InputBox';

export const ColDefs = {
    MinAmount:'MinAmount',
    MaxAmount:'MaxAmount',
}

const MinMaxAmount = memo((props)=>{
    let disabled = !props.isEdit;
    let minAmount = [];
    let maxAmount = [];

    if(props.Datas){
        Object.keys(props.Datas).forEach(k=>{
            let amountInfo = props.Datas[k];
            minAmount.push(
                <InputBox
                    key={`${props.MinAmountTitle}-${k}-${ColDefs.MinAmount}`}
                    SubTitle={window.i18n[`lbl_Currency${k}`]} 
                    value={amountInfo[ColDefs.MinAmount] === null || amountInfo[ColDefs.MinAmount] === undefined ? '' : amountInfo[ColDefs.MinAmount]} 
                    OnChange={props.HandleStateData.bind(null, k)} 
                    dataColumn={ColDefs.MinAmount} 
                    Currency={k}
                    type={'Number'}
                    Disabled={disabled}
                />
            );

            maxAmount.push(
                <InputBox
                    key={`${props.MaxAmountTitle}-${k}-${ColDefs.MaxAmount}`}
                    SubTitle={window.i18n[`lbl_Currency${k}`]} 
                    value={amountInfo[ColDefs.MaxAmount] === null || amountInfo[ColDefs.MaxAmount] === undefined ? '' : amountInfo[ColDefs.MaxAmount]}
                    OnChange={props.HandleStateData.bind(null, k)}
                    dataColumn={ColDefs.MaxAmount}
                    Currency={k}
                    type={'Number'}
                    Disabled={disabled}
                />
            );
        })
    }

    return(
        <div className="filter">
            <h4>{props.SectionTitle}</h4>
            <div className="row-flex">
                <div className="input-item">
                    <label className="label">{props.MinAmountTitle}:</label>
                    {minAmount}
                </div>
                <div className="input-item">
                    <label className="label">{props.MaxAmountTitle}:</label>
                    {maxAmount}
                </div>
            </div>
        </div>
    )
})

export default MinMaxAmount;