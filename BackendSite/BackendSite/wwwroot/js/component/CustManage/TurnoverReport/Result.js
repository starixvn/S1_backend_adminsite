import {number_format} from '../../commJS/common';
const Result = (props)=>{
    let total_stake = 0;
    let total_winLost = 0;
    let rows = props.Datas.length === 0 ? <tr><td>No Data.</td><td/><td/></tr> : props.Datas.map((d, dIdx)=>{
        total_stake += d.stake;
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
                            <th>Total - Turnover</th>
                            <th>Total - Winlost</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>{number_format(total_stake)}</td>
                            <td>{number_format(total_winLost)}</td>
                        </tr>
                    </tbody>
                </table>                
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th>GameName</th>
                            <th>Turnover</th>
                            <th>WinLost</th>
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