import React, {useState, useEffect} from 'react';
import PeriodSetting, {ColDefs as ColDefsPeriodSetting, PeriodType, WeekOptions} from './PeriodSetting';
import ReactDOM from 'react-dom';
import MinMaxAmount, {ColDefs as ColDefsMinMaxAmount } from './MinMaxAmount';
import GameRules from './GameRules';
import WebAPI from '../../../module/WebAPI';
import qs from 'qs';
import Button, {BtnClass} from './Component/Button';

let TpList = {};
let Currencies = [];
let CashBackId = null;

let initData = null;

const ClickType = {
    save:'save',
    edit:'edit',
    cancel:'cancel'
}

const TimeZoneOffSet = ()=>{
    return -(new Date().getTimezoneOffset()/60);
}

const Panel = ()=>{
    const [isEdit, setIsEdit] = useState(false);
    const [PeriodSettingDatas, SetPeriodSettingData] = useState({
        [ColDefsPeriodSetting.Week]:1
    });

    const [CashbackLimitDatas, SetCashbackLimitDatas] = useState({});
    const [BetLimitDatas, SetBetLimitDatas] = useState({});
    const [DefaultPercentage, setDefaultPercentage] = useState('');
    const [GameRuleDatas, setGameRuleDatas] = useState(null);
    const [LockedGames, setGameLock] = useState(null);

    const HandlePeriodSetting = (v, dataColumn)=>{
        SetPeriodSettingData(prevState=>({...prevState, [dataColumn]:v}));
    }

    const HandleCashbackLimit = (currency, v, dataColumn) => {
        SetCashbackLimitDatas((prevState) => ({
            ...prevState,
            [currency]: {
                ...prevState[currency],
                [dataColumn]: v,
            },
        }));
    };

    const HandleBetLimit = (currency, v, dataColumn) => {
        SetBetLimitDatas((prevState) => ({
            ...prevState,
            [currency]: {
                ...prevState[currency],
                [dataColumn]: v,
            },
        }));
    };

    const SetGameRules = (value, tpId)=>{
        setGameRuleDatas(prevState=>({
            ...prevState, 
            [tpId]:value
        }));
    }

    const HandleLockGame = (tpId)=>{
        setGameLock(prevState=>{
            return {...prevState, [tpId]: !prevState[tpId]}
        });
    }

    const SetDefaultPercentage = (v)=>{
        setDefaultPercentage(v);
        // setGameRuleDatas(prevState=>{
        //     Object.keys(prevState).forEach(k=>{
        //         if(LockedGames[k] !== true && prevState[k]===null || prevState[k]===undefined || prevState[k]===''){
        //             prevState[k]=v;
        //         }
        //     })
        //     return {...prevState};
        // });
    }

    useEffect(()=>{
        let param = {
            Url: 'BonusCashBack/GetCashBackBonus'
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0){
                let data = res.data.data;
                CashBackId = data.id === undefined ? null : data.id;

                let hour = data.payHour;
                let minute = data.payMinute;
                if(data.id !== undefined && data.id !== null){
                    let _timeZoneOffset = TimeZoneOffSet();
                    if(_timeZoneOffset !== data.timeZone){
                        let offset = _timeZoneOffset - data.timeZone;
                        let offset_point = Math.round((offset % 1)*100)/100;

                        let offsetMinute = offset_point * 60;
                        let tmpMinute = minute + Math.abs(offsetMinute);

                        minute = Math.round(Math.abs(((tmpMinute/60)%1)*60));
                        
                        let offsetHour = Math.sign(offset) === -1 ? -(Math.floor(Math.abs(offset))) : Math.floor(offset);
                        hour = offsetHour + Math.floor(tmpMinute/60) + hour + (Math.sign(offsetMinute)===-1? -1 : 0);
                    }
                }

                SetPeriodSettingData({
                    [ColDefsPeriodSetting.PeriodType]:data.period,
                    [ColDefsPeriodSetting.Hour]:hour,
                    [ColDefsPeriodSetting.Minute]:minute,
                    [ColDefsPeriodSetting.Week]:data.dayOfWeek===null || data.dayOfWeek === undefined ? 1 : data.dayOfWeek,
                    [ColDefsPeriodSetting.WithdrawalCondition]:data.withdrawCondition
                });

                if(data.defaultPercentage !== null){
                    setDefaultPercentage(data.defaultPercentage);
                }

                let betLimit = {};
                let cashbackLimit = {};
                data.bonusCashBackMinMax.forEach(c=>{
                    Currencies.push(c.currencyId);
                    betLimit[c.currencyId] = {
                        [ColDefsMinMaxAmount.MinAmount]:c.minBet ? c.minBet : 0,
                        [ColDefsMinMaxAmount.MaxAmount]:c.maxBet ? c.maxBet : 0
                    };

                    cashbackLimit[c.currencyId] = {
                        [ColDefsMinMaxAmount.MinAmount]:c.minPay ? c.minPay : 0,
                        [ColDefsMinMaxAmount.MaxAmount]:c.maxPay ? c.maxPay : 0
                    }
                });
                SetBetLimitDatas({...betLimit});
                SetCashbackLimitDatas({...cashbackLimit});

                let gameRulesResult = {};
                let gameLock = {};
                data.bonusCashBackGames.forEach(t=>{
                    gameRulesResult[t.id] = t.percentage;
                    TpList[t.id] = t.tpName;
                    gameLock[t.id] = CashBackId === null ? 0 : t.status===0;
                });
                setGameLock(gameLock);
                setGameRuleDatas(gameRulesResult);
            } else {
                alert(res.data.message ?? 'failed.');
            }
        });
    },[])

    const HandleSubmit = ()=>{
        let gameRules = [];
        let minMaxLimit = [];

        Currencies.forEach(c=>{
            minMaxLimit.push({
                CurrencyId:c,
                MinPay:CashbackLimitDatas[c].MinAmount ? CashbackLimitDatas[c].MinAmount : 0,
                MaxPay:CashbackLimitDatas[c].MaxAmount ? CashbackLimitDatas[c].MaxAmount : 0,
                MinBet:BetLimitDatas[c].MinAmount ? BetLimitDatas[c].MinAmount : 0,
                MaxBet:BetLimitDatas[c].MaxAmount ? BetLimitDatas[c].MaxAmount : 0
            })
        });

        Object.keys(GameRuleDatas).forEach(k=>{
            gameRules.push({
                Id:k,
                //Percentage: (GameRuleDatas[k]===undefined || GameRuleDatas[k]===null || GameRuleDatas[k]==='') && !LockedGames[k] ? DefaultPercentage : GameRuleDatas[k],
                Percentage: GameRuleDatas[k],
                Status:LockedGames[k] === true ? 0 : 1
            });
        });

        let param = {
            Url: 'BonusCashBack/SetCashBackBonus',
            Data: qs.stringify({
                _CashBackBonus:{
                    Id:CashBackId,
                    Period:PeriodSettingDatas[ColDefsPeriodSetting.PeriodType],
                    DayOfWeek: PeriodSettingDatas[ColDefsPeriodSetting.PeriodType]=== PeriodType.Weekly ? PeriodSettingDatas[ColDefsPeriodSetting.Week] : null,
                    PayHour:PeriodSettingDatas[ColDefsPeriodSetting.Hour] === '' ? 0 : PeriodSettingDatas[ColDefsPeriodSetting.Hour],
                    PayMinute:PeriodSettingDatas[ColDefsPeriodSetting.Minute] === '' ? 0 : PeriodSettingDatas[ColDefsPeriodSetting.Minute],
                    WithdrawCondition:PeriodSettingDatas[ColDefsPeriodSetting.WithdrawalCondition],
                    DefaultPercentage:DefaultPercentage,
                    TimeZone: TimeZoneOffSet()
                },
                _CashBackGames:gameRules,
                _CashBackMinMaxAmounts:minMaxLimit
            })
        };

        WebAPI.PostData(param).then((res) => {
            setProcess(false);
            if (res.data.errorCode === 0){
                setIsEdit(false);
                alert('submit successfully');
                CashBackId = CashBackId === null ? res.data.data : CashBackId;
            } else {
                alert(res.data.message ?? 'failed.');
            }
        }).catch((err)=>{
            setProcess(false);
        });
    }

    const [isProcess, setProcess] = useState(false);
    const BtnOnClick = (_clickType)=>{
        if(!isProcess){
            if(_clickType === ClickType.edit){
                initData = null;
                initData = {
                    PeriodSettingDatas: {...PeriodSettingDatas},
                    CashbackLimitDatas: {...CashbackLimitDatas},
                    BetLimitDatas:{...BetLimitDatas},
                    DefaultPercentage:DefaultPercentage,
                    GameRuleDatas:GameRuleDatas === null ? null : {...GameRuleDatas}
                };
    
                setIsEdit(true);
            }else{
                if(_clickType===ClickType.save){
                    if(window.confirm("Are you sure you want to submit?")){
                        let check = true;
                        for(let idx in Currencies){
                            let c = Currencies[idx];
                            let MinPay = Number(CashbackLimitDatas[c].MinAmount);
                            let MaxPay = Number(CashbackLimitDatas[c].MaxAmount);
                            let MinBet = Number(BetLimitDatas[c].MinAmount);
                            let MaxBet = Number(BetLimitDatas[c].MaxAmount);
                            if(!(MinPay===0 && MaxPay===0) && MinPay > MaxPay){
                                check = false;
                                alert(`${window.i18n[`lbl_Currency${c}`]}'s Min Limit Cashback Amount more than Max Limit Cashback Amount`)
                                break;
                            }else if((MinBet === 0 && MaxBet === 0) && MinBet > MaxBet){
                                check = false;
                                alert(`${window.i18n[`lbl_Currency${c}`]}'s Min Bet Amount more than Max Bet Amount`)
                                break;
                            }
                        }

                        if(check){
                            setProcess(true);
                            HandleSubmit();
                        }
                    }
                }else{
                    SetPeriodSettingData(prevState=>({...initData.PeriodSettingDatas}));
                    SetCashbackLimitDatas(prevState=>({...initData.CashbackLimitDatas}));
                    SetBetLimitDatas(prevState=>({...initData.BetLimitDatas}));
                    setDefaultPercentage(prevState=>(DefaultPercentage));
                    setGameRuleDatas(prevState=>(initData.GameRuleDatas));
                    setIsEdit(false);
                }
            }
        }
    }

    return (
        <div className="container-fluid Cashback-Period-Setting">
            {
                isEdit ? null : (
                    <div className="row-flex" style={{marginTop:'15px'}}>
                        <div className="button-group">
                            <Button _className={BtnClass.primary} displayText='Edit' onClick={BtnOnClick.bind(null, ClickType.edit)}/>
                        </div>
                    </div>
                )
            }
            
            <Section Title={"Cashback Period Setting"}>
                <PeriodSetting
                    HandleStateData={HandlePeriodSetting}
                    Datas={PeriodSettingDatas}
                    isEdit={isEdit}
                />
            </Section>

            <Section Title={"Cashback Limit Amount"}>
                <MinMaxAmount
                    MinAmountTitle="Min Limit Cashback Amount"
                    MaxAmountTitle="Max Limit Cashback Amount"
                    Datas={CashbackLimitDatas}
                    HandleStateData={HandleCashbackLimit}
                    isEdit={isEdit}
                />
            </Section>

            <Section Title={"Bet Limit Amount"}>
                <MinMaxAmount
                    MinAmountTitle="Min Bet Amount"
                    MaxAmountTitle="Max Bet Amount"
                    Datas={BetLimitDatas}
                    HandleStateData={HandleBetLimit}
                    isEdit={isEdit}
                />
            </Section>

            <Section Title={"Cashback Game Rules"}>
                <GameRules
                    GameRuleDatas={GameRuleDatas}
                    SetGameRules={SetGameRules}
                    TpList={TpList}
                    DefaultPercentage={DefaultPercentage}
                    SetDefaultPercentage={SetDefaultPercentage}
                    isEdit={isEdit}
                    LockedGames={LockedGames}
                    HandleLockGame={HandleLockGame}
                />
            </Section>
            {
                isEdit ? ( <BtnBar BtnOnClick={BtnOnClick} Disable={isProcess}/> ) : null
            }
        </div>
    );
}

const Section = (props)=>{
    return(
        <>
            <h4 className="page-name">{props.Title}</h4>
            {props.children}
        </>
    )
}

const BtnBar = ({BtnOnClick, Disable})=>{
    return(
        <div className="row-flex">
            <div className="button-group">
                <Button _className={BtnClass.primary} displayText='Save' onClick={BtnOnClick.bind(null, ClickType.save)} Disable={Disable}/>
                <Button _className={BtnClass.secondary} displayText='Cacnel' onClick={BtnOnClick.bind(null, ClickType.cancel)} Disable={Disable}/>
            </div>
        </div>
    )
}

export default Panel;

if(process.env.NODE_ENV==='production'){
    ReactDOM.render(<Panel />, document.getElementById('content'))
}