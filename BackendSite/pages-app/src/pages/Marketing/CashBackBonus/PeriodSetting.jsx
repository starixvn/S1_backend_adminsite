import React, {memo} from 'react';
import InputBox from './Component/InputBox';
import RadioBtn from './Component/RadioBox';
import SelectBox from './Component/SelectBox';
import moment from 'moment';

export const ColDefs = {
    PeriodType:'PeriodType',
    Hour:'Hour',
    Minute:'Minute',
    Week:'Week',
    WithdrawalCondition:'WithdrawalCondition'
}

export const PeriodType = {
    Daily:1,
    Weekly: 2
}

const defColPeriod = [
    {
        id:'Period-Daily',
        displayText:'Daily',
        value:PeriodType.Daily,
        name:'Execute-Period',
        column:ColDefs.PeriodType
    },
    {
        id:'Period-Weekly',
        displayText:'Weekly',
        value:PeriodType.Weekly,
        name:'Execute-Period',
        column:ColDefs.PeriodType
    }
]

const PeriodSetting = memo((props)=>{
    let disabled = !props.isEdit;
    return(
        <div className="filter">
            <div className="row-flex">
                <div className="input-item">
                    <label className="label">Cashback Period</label>
                </div>
            </div>
            <div className="row-flex">
                {
                    defColPeriod.map(col=>{
                        return (
                            <RadioBtn 
                                key={col.id}
                                Id={col.id} 
                                DisplayText={col.displayText} 
                                Value={col.value} 
                                Name={`Execute-Period`} 
                                OnChange={props.HandleStateData.bind(null, col.value, col.column)} 
                                Checked={props.Datas[col.column] === col.value}
                                Disabled={disabled}
                            />
                        );
                    })
                }
            </div>
            <PeriodOpt 
                _periodType={props.Datas[ColDefs.PeriodType]}
                _hour={props.Datas[ColDefs.Hour]}
                _minute={props.Datas[ColDefs.Minute]}
                _week={props.Datas[ColDefs.Week]}
                HandleStateData={props.HandleStateData}
                Disabled={disabled}
            />
            <WithdrawalCondition HandleStateData={props.HandleStateData} _withdrawalCondition={props.Datas[ColDefs.WithdrawalCondition]} Disabled={disabled}/>
        </div>
    )
})

export const WeekOptions = {
    1:'Monday',
    2:'Tuesday',
    3:'Wednesday',
    4:'Thursday',
    5:'Friday',
    6:'Saturday',
    7:'Sunday'
}

const PeriodOpt = ({_periodType, _hour, _minute, _week, HandleStateData, Disabled})=>{
    let strHour = _hour === undefined ? '' : typeof(_hour) === 'string' ? _hour.padStart(2, '0') : _hour?.toString().padStart(2, '0');
    let strMinute = _minute === undefined ? '' : typeof(_minute) === 'string' ? _minute.padStart(2, '0') : _minute?.toString().padStart(2, '0');
    let displayExecTime = _hour === null || _hour===undefined ?  '' : `${strHour}:${strMinute}`;
    let weekOpt = Object.keys(WeekOptions).map(k=>{
        return {value:k, displayText:WeekOptions[k]}
    });
    return (
        <>
            <div className="row-flex">
                    <label className="label">Pay Time {" (" + moment().format('Z') + ") "}<span className="bule-bold">{_periodType === PeriodType.Weekly ? WeekOptions[_week] : ''}  {displayExecTime}</span></label>
            </div>
            <div className="row-flex">
                <InputBox
                    OnChange={HandleStateData}
                    value={_hour===null || _hour===undefined ? '' : _hour}
                    dataColumn={ColDefs.Hour}
                    regex={/^0*([0-9]|1[0-9]|2[0-3])$/}
                    Disabled={Disabled}
                />
                <InputBox
                    OnChange={HandleStateData}
                    value={_minute===null || _minute===undefined ? '' : _minute}
                    dataColumn={ColDefs.Minute}
                    regex={/^0*([0-9]|[1-4][0-9]|5[0-9])$/}
                    Disabled={Disabled}
                />
                {_periodType === PeriodType.Weekly ? (
                    <SelectBox
                        Options={weekOpt}
                        HandleChange={HandleStateData}
                        dataColumn={ColDefs.Week}
                        SelectedValue={_week}
                        Disabled={Disabled}
                    />
                ) : null}
            </div>
        </>
    );
}

const WithdrawalCondition = ({HandleStateData, _withdrawalCondition, Disabled})=>{
    return(
        <div className="row-flex">
            <InputBox
                Title={"Withdraw Condition(Cashback Amount X Times):"}
                ClassName={"input-item no-wrap-370"}
                OnChange={HandleStateData} 
                value={_withdrawalCondition === null || _withdrawalCondition === undefined ? '' : _withdrawalCondition} 
                dataColumn={ColDefs.WithdrawalCondition}
                type={'Number'}
                Disabled={Disabled}
            />
        </div>
    )
}

export default PeriodSetting;