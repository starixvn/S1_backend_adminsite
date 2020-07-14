import React, { useState, useEffect, useRef } from 'react';
import ReactDOM from 'react-dom';
import Result from './result';
import WebAPI from '../../../module/WebAPI';
import qs from 'qs';

const Panel = ()=>{
    const [stateData, setData] = useState([]);
    const [processing, setProcessing] = useState(false);
    const isMountedRef = useRef(null);
	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
    });
    
    useEffect(() => {
        let param = {
            Url: 'BonusInfo/GetPendingBonusList'
        };
        WebAPI.PostData(param).then(res=>{
            if(res.data.errorCode === 0 && isMountedRef.current){
                setData(res.data.data);
            }
        })
    }, [])

    const GetPendingBonus = ()=>{
        let param = {
            Url: 'BonusInfo/GetPendingBonusList'
        };
        WebAPI.PostData(param).then(res=>{
            if(res.data.errorCode === 0 && isMountedRef.current){
                setData(res.data.data);
            }
            setProcessing(false);
        }, err=>{
            setProcessing(false)
        })
    }

    const ClickCancel = (idx)=>{
        if(!processing){
            setProcessing(true);
            let param = {
                Url: 'BonusInfo/BonusCancel',
                Data: qs.stringify({BonusId: stateData[idx].bonusId})
            };
            WebAPI.PostData(param).then(res=>{
                if(res.data.errorCode === 0 && isMountedRef.current){
                    GetPendingBonus();
                }
            }, err=>{
                setProcessing(false);
            });
        }
    }

    const ClickTransfer = (idx)=>{
        if(!processing){
            setProcessing(true);
            let param = {
                Url: 'BonusInfo/BonusTransfer',
                Data: qs.stringify({CustId: stateData[idx].custId})
            };
            WebAPI.PostData(param).then(res=>{
                if(res.data.errorCode === 0 && isMountedRef.current){
                    GetPendingBonus();
                }
            }, err=>{
                setProcessing(false);
            });
        }
    }

    const ClickTransferAll = ()=>{
        if(!processing){
            if(!processing){
                setProcessing(true);
                let param = {
                    Url: 'BonusInfo/BonusTransferAll'
                };
                WebAPI.PostData(param).then(res=>{
                    if(res.data.errorCode === 0 && isMountedRef.current){
                        GetPendingBonus();
                    }
                }, err=>{
                    setProcessing(false);
                });
            }
        }
    }

    return(
        <div className="container-fluid">
            <h4 className="page-name">Pending Bonus</h4>
            <button className="btn btn-search" onClick={ClickTransferAll} style={{marginBottom: '20px'}} disabled={processing}>
				Transfer All
			</button>
            <Result Datas={stateData} ClickCancel={ClickCancel} ClickTransfer={ClickTransfer} processing={processing}/>
        </div>
    )
}

export default Panel;

if(process.env.NODE_ENV==='production'){
    ReactDOM.render(<Panel />, document.getElementById('content'))
}