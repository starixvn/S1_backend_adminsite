import React, {useState, useRef, useEffect} from "react";
import ReactDOM from 'react-dom';
import Filter from './Filter';
import Result from './Result';
import WebAPI from '../../../module/WebAPI';
import moment from 'moment';
import qs from 'qs';
import {number_format, number_toFixed} from '../../../module/Formatting/NumberFormat';

const App = ()=>{
    const [stateData, setData] = useState({
        SearchInfo:{
            Currency:0,
            StartDate:null,
            EndDate:null
        },
        Datas:null,
    });

    const [stateFilter, setFilter] = useState({
        'StartDate':null,
        'EndDate':null,
        'Currency':0
    });

    const [stateRef] = useState({
        f_trigger_Start:useRef(null),
        f_trigger_End:useRef(null),
        selectdate_end:useRef(null),
        selectdate_start:useRef(null),
    });

    useEffect(() => {
        let f_trigger_Start = stateRef.f_trigger_Start.current;
        let f_trigger_End = stateRef.f_trigger_End.current;
        let selectdate_end = stateRef.selectdate_end.current;
        let selectdate_start = stateRef.selectdate_start.current;
        
        /*eslint-disable */
        Calendar.setup({
            inputField: selectdate_end, // id of the input field
            ifFormat: "mm/dd/y",        // format of the input field
            button: f_trigger_End,  // trigger for the calendar (button ID)
            singleClick: true              // double-click mode
        });

        Calendar.setup({
            inputField: selectdate_start, // id of the input field
            ifFormat: "mm/dd/y",        // format of the input field
            button: f_trigger_Start,  // trigger for the calendar (button ID)
            singleClick: true              // double-click mode
        });
        /*eslint-enable */     
    }, [stateRef]);

    const Filter_OnChange = (v, dataColumn)=>{
        setFilter({
            ...stateFilter,
            [dataColumn]:v
        });
    }

    const Filter_Submit = ()=>{
        let startDate = stateRef.selectdate_start.current.value;
        let endDate = stateRef.selectdate_end.current.value;

        let param = {
            Url: 'WinLostReport/GetLicWinLostReport',
            Data:qs.stringify({
                StartDate:startDate,
                EndDate:endDate,
                Currency:stateFilter.Currency
            })
        };

        setData({
            ...stateData,
            Datas:null,
            GameList:[]
        });

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                let result = [];
                let gameList = [];
                if(res.data.data.length!==0){
                    res.data.data.forEach(elem => {
                        if(!gameList.find(x=>x.type===elem.gameType)){
                            gameList.push({
                                type:elem.gameType,
                                name:elem.gameName
                            });
                        }

                        let date = moment(elem.settledDate).format('l');
                        let getItem = result.find(x=> x.currencyId === elem.currencyId && x.date === date);
                        let gameStakeColDef = `stake_game_${elem.gameType}`;
                        let gameWinLostColDef = `winlost_game_${elem.gameType}`;
                        if(getItem){
                            getItem.stakeSummary += elem.stakeSummary;
                            getItem.winlostAmount += elem.winlostAmount;
                            getItem[gameStakeColDef] = getItem[gameStakeColDef] ? getItem[gameStakeColDef] + elem.stakeSummary : elem.stakeSummary;
                            getItem[gameWinLostColDef] = getItem[gameWinLostColDef] ? getItem[gameWinLostColDef] + elem.winlostAmount : elem.winlostAmount;
                        }else{
                            result.push({
                                'date':date,
                                'currencyId':elem.currencyId,
                                'currencyName':elem.currencyName,
                                'stakeSummary':elem.stakeSummary,
                                'winlostAmount':elem.winlostAmount,
                                [gameStakeColDef]:elem.stakeSummary,
                                [gameWinLostColDef]:elem.winlostAmount                           
                            });
                        }
                    });
                }

                setData({
                    ...stateData,
                    SearchInfo:{
                        Currency:stateFilter.Currency,
                        StartDate:startDate,
                        EndDate:endDate
                    },
                    Datas:[...result.map(item=>{
                            Object.keys(item).forEach(key => {
                                if(key.startsWith('stake') || key.startsWith('winlost')){
                                    item[key] = number_format(number_toFixed(item[key], 2));
                                }
                            });

                            return {...item}
                        })
                    ],
                    GameList:[...gameList]
                });
            }else{
                alert(res.data.message)
            }
        });
    }

    return (
        <div style={{ width: "100%", height: "100%", padding: "15px" }}>
            <div className="page-title">
                Licensee Win Lost
            </div>
            <Filter filterInfo={stateFilter} refInfo={stateRef} onChange={Filter_OnChange} ClickSubmit={Filter_Submit}/>
            {stateData.Datas === null ? null : <Result {...stateData} />}
        </div>
    );
}

export default App;

if(process.env.NODE_ENV==='production'){
    ReactDOM.render(<App />, document.getElementById('content'))
}