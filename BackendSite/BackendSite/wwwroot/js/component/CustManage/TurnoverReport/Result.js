import {number_format} from '../../commJS/common';
const Result = (props)=>{
    let total_settledForValid_stake = 0;
    let total_settledForValidNotRollover_stake = 0;
    let total_winLost = 0;
    let rows = props.Datas.length === 0 ? <tr><td>No Data.</td><td/><td/></tr> : props.Datas.map((d, dIdx)=>{
        if(d.settlementType == 1){
            total_settledForValid_stake += d.stake;
        }
        if(d.settlementType == 3){
            total_settledForValidNotRollover_stake += d.stake;
        }
        total_winLost +=d.winlostAmount;
        return(
            <tr key={`result-${dIdx}`}>
                <td>{d.gameName}</td>
                <td>{number_format(d.stake)}</td>
                <td>{number_format(d.winlostAmount)}</td>
            </tr>
        )
    })
    return(
        <>
            <div className="table">
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th>Total - Turnover (Settled)</th>
                            <th>Total - Turnover (NotRollover)</th>
                            <th>Total - WinLoss</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>{number_format(total_settledForValid_stake)}</td>
                            <td>{number_format(total_settledForValidNotRollover_stake)}</td>
                            <td>{number_format(total_winLost)}</td>
                        </tr>
                    </tbody>
                </table>                
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th>GameName</th>
                            <th>Turnover</th>
                            <th>WinLoss</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows}
                    </tbody>
                </table>
            </div>
        </>
    )
}

export default Result;